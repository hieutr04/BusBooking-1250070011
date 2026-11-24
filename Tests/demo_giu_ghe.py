"""Demo HTTP Bus Center trên SQL Server kiểm thử. Chỉ cần Python và sqlcmd."""

import argparse
import csv
import json
import math
import re
import subprocess
import sys
import time
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timezone
from html.parser import HTMLParser
from http.cookiejar import CookieJar
from pathlib import Path
from threading import Barrier
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode, urlsplit
from urllib.request import HTTPCookieProcessor, ProxyHandler, Request, build_opener
from uuid import UUID, uuid4


class LoginToken(HTMLParser):
    token = None

    def handle_starttag(self, tag, attrs):
        fields = dict(attrs)
        if tag == "input" and fields.get("name") == "__RequestVerificationToken":
            self.token = fields.get("value")


class Session:
    def __init__(self, base_url, email, password):
        self.base_url = base_url
        cookies = CookieJar()
        self.client = build_opener(ProxyHandler({}), HTTPCookieProcessor(cookies))
        status, html = self.request("/Account/Login")
        parser = LoginToken()
        parser.feed(html)
        require(status == 200 and parser.token, "Không lấy được token đăng nhập.")
        self.request("/Account/Login", form={
            "Email": email, "Password": password, "__RequestVerificationToken": parser.token
        })
        require(any(c.name == "BusBooking.Auth" for c in cookies), f"Đăng nhập fixture thất bại: {email}")
        self.csrf = self.get_json("/api/seats/csrf")["token"]

    def request(self, path, *, payload=None, form=None):
        headers = {}
        data = None
        if payload is not None:
            headers = {"Content-Type": "application/json", "X-CSRF-TOKEN": self.csrf}
            data = json.dumps(payload, ensure_ascii=False).encode("utf-8")
        elif form is not None:
            headers = {"Content-Type": "application/x-www-form-urlencoded"}
            data = urlencode(form).encode("utf-8")
        try:
            with self.client.open(Request(self.base_url + path, data=data, headers=headers), timeout=60) as res:
                return res.status, res.read().decode("utf-8", errors="replace")
        except HTTPError as error:
            with error:
                return error.code, error.read().decode("utf-8", errors="replace")

    def get_json(self, path):
        status, body = self.request(path)
        require(status == 200, f"GET {path} trả HTTP {status}.")
        return json.loads(body)


def require(condition, message):
    if not condition:
        raise RuntimeError(message)


def sql(server, database, query):
    command = ["sqlcmd", "-S", server, "-d", database, "-E", "-C", "-b", "-l", "5",
               "-h", "-1", "-W", "-s", "|", "-Q", "SET NOCOUNT ON; " + query]
    result = subprocess.run(command, capture_output=True, text=True, errors="replace", timeout=15)
    require(result.returncode == 0, f"sqlcmd thất bại: {result.stderr.strip() or result.stdout.strip()}")
    return result.stdout.strip().split("|")


def seat_state(server, database, seat_id):
    status, holds, tickets, paid = sql(server, database, f"""
        SELECT s.Status,
            (SELECT COUNT(*) FROM dbo.HoldSeats i JOIN dbo.SeatHolds h ON h.Id=i.SeatHoldId
                WHERE i.TripSeatId=s.Id AND h.Status='Active' AND h.ExpiresAtUtc>SYSUTCDATETIME()),
            (SELECT COUNT(*) FROM dbo.Tickets t WHERE t.TripSeatId=s.Id AND t.IsActive=1),
            (SELECT COUNT(*) FROM dbo.Tickets t JOIN dbo.Bookings b ON b.Id=t.BookingId
                WHERE t.TripSeatId=s.Id AND t.IsActive=1 AND b.Status='Active' AND b.PaymentExpiresUtc IS NULL
                AND b.Total<=(SELECT COALESCE(SUM(p.Amount),0) FROM dbo.Payments p WHERE p.BookingId=b.Id))
        FROM dbo.TripSeats s WHERE s.Id={seat_id};
    """)
    return status, int(holds), int(tickets), int(paid)


def hold(session, trip, seat):
    return session.request("/api/seats/hold", payload={
        "tripId": trip, "seatIds": [seat], "requestKey": str(uuid4())
    })


def release(session, hold_id):
    status, _ = session.request("/api/seats/release", payload={"holdId": hold_id})
    require(status == 200, f"Nhả lượt giữ trả HTTP {status}.")


def confirm(session, hold_id, seat):
    return session.request("/api/seats/confirm", payload={
        "holdId": hold_id, "contactName": "Khách demo Python", "phone": "0900000000",
        "passengers": [{"tripSeatId": seat, "name": "Khách demo Python"}]
    })


def race(sessions, trip, seat):
    barrier = Barrier(len(sessions))

    def attempt(index):
        barrier.wait(timeout=30)
        started = time.perf_counter()
        try:
            status, body = hold(sessions[index], trip, seat)
        except (URLError, OSError):
            status, body = 0, ""
        return index, status, (time.perf_counter() - started) * 1000, body

    with ThreadPoolExecutor(max_workers=len(sessions)) as pool:
        return list(pool.map(attempt, range(len(sessions))))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--fixture", type=Path, required=True)
    parser.add_argument("--url", default="http://127.0.0.1:5190")
    parser.add_argument("--sql-server", default=r"lpc:.\SQLEXPRESS01")
    parser.add_argument("--rounds", nargs="+", type=int, choices=[20, 50, 100], default=[20, 50, 100])
    parser.add_argument("--confirm", action="store_true", help="Xác nhận và hoàn tất QR mô phỏng ở đợt cuối.")
    parser.add_argument("--wait-300", action="store_true", help="Đo nhả ghế thật trên ghế riêng, chờ khoảng 5 phút.")
    args = parser.parse_args()
    url = urlsplit(args.url)
    require(url.scheme == "http" and url.hostname in {"localhost", "127.0.0.1"} and
            not (url.path.strip("/") or url.query or url.fragment or url.username), "Chỉ dùng HTTP localhost của bản test.")
    base = args.url.rstrip("/")
    fixture = json.loads(args.fixture.read_text(encoding="utf-8-sig"))
    database = fixture["database"]
    run = fixture["run"]
    require(re.fullmatch(r"[A-Za-z0-9_]+_Tests", database, re.IGNORECASE), "Cần database riêng kết thúc bằng _Tests.")
    require(re.fullmatch(r"\d{14}[0-9a-f]{5}", run), "Mã fixture không hợp lệ.")
    trip = int(fixture["tripId"])
    seats = [int(seat) for seat in fixture["seatIds"]]
    require(len(seats) >= len(args.rounds) + int(args.wait_300), "Fixture thiếu ghế cho các kịch bản.")
    require(sql(args.sql_server, database, f"""
        SELECT COUNT(*) FROM dbo.Trips t JOIN dbo.Routes r ON r.Id=t.BusRouteId
        WHERE t.Id={trip} AND r.Origin=N'Test-{run}';
    """) == ["1"], "SQL Server không có chuyến của fixture.")
    password = fixture["customers"][0]["password"]
    print(f"Đăng nhập {max(args.rounds)} khách giả lập...", flush=True)
    sessions = [Session(base, f"sqltest-{run}-{i}@busbooking.test", password) for i in range(max(args.rounds))]
    snapshot = sessions[0].get_json(f"/api/seats/{trip}")
    require(snapshot["canBook"] and {s["id"] for s in snapshot["seats"]} == set(seats),
            "Web không khớp chuyến/ghế của fixture hoặc chuyến đã đóng bán.")
    output = args.fixture.parent / datetime.now(timezone.utc).strftime("python-demo-%Y%m%d-%H%M%S-%f")
    output.mkdir()
    print(f"Kết quả: {output}", flush=True)
    all_ok = True
    with (output / "tong-hop.csv").open("w", newline="", encoding="utf-8-sig") as summary_file, (
        output / "yeu-cau.csv"
    ).open("w", newline="", encoding="utf-8-sig") as samples_file:
        summary, details = csv.writer(summary_file), csv.writer(samples_file)
        summary.writerow(["so_request", "trip_seat_id", "http_200", "http_409", "loi_khac", "trung_binh_ms",
                          "p95_ms", "hold_hieu_luc", "ve_hieu_luc", "dat_trung", "ket_qua"])
        details.writerow(["so_request", "trip_seat_id", "client", "http_status", "response_ms"])
        for round_index, n in enumerate(args.rounds):
            seat = seats[round_index]
            require(seat_state(args.sql_server, database, seat) == ("Available", 0, 0, 0),
                    f"Ghế {seat} đã được sử dụng. Hãy tạo fixture mới.")
            samples = race(sessions[:n], trip, seat)
            times = sorted(ms for _, _, ms, _ in samples)
            winners = [sample for sample in samples if sample[1] == 200]
            conflicts = sum(code == 409 for _, code, _, _ in samples)
            errors = n - len(winners) - conflicts
            status, holds, tickets, _ = seat_state(args.sql_server, database, seat)
            ok = (len(winners), conflicts, errors, status, holds, tickets) == (1, n - 1, 0, "Held", 1, 0)
            p95 = times[math.ceil(0.95 * n) - 1]
            summary.writerow([n, seat, len(winners), conflicts, errors, round(sum(times) / n, 2), round(p95, 2),
                              holds, tickets, max(0, holds + tickets - 1), "PASS" if ok else "FAIL"])
            details.writerows([n, seat, i, code, round(ms, 2)] for i, code, ms, _ in samples)
            summary_file.flush()
            samples_file.flush()
            print(f"{n:3} request | 200={len(winners)} | 409={conflicts} | lỗi={errors} | P95={p95:.1f} ms"
                  f" | SQL holds={holds}, vé={tickets} | {'PASS' if ok else 'FAIL'}", flush=True)
            all_ok &= ok
            for index, _, _, body in winners:
                hold_id = str(UUID(json.loads(body)["id"]))
                if ok and args.confirm and round_index == len(args.rounds) - 1:
                    code, result = confirm(sessions[index], hold_id, seat)
                    require(code == 200, f"Xác nhận vé trả HTTP {code}.")
                    booking = str(UUID(json.loads(result)["id"]))
                    code, retry = confirm(sessions[index], hold_id, seat)
                    require(code == 200 and json.loads(retry)["id"] == booking, "Thử lại tạo ra kết quả khác.")
                    sessions[index].request(f"/Bookings/SimulatePayment/{booking}",
                                            form={"__RequestVerificationToken": sessions[index].csrf})
                    require(seat_state(args.sql_server, database, seat) == ("Booked", 0, 1, 1),
                            "SQL chưa ghi nhận đúng một vé đã thanh toán. Kiểm tra cấu hình DemoPayment.")
                    print("PASS xác nhận lặp và QR mô phỏng: SQL có đúng một vé đã thanh toán.", flush=True)
                else:
                    release(sessions[index], hold_id)

    if args.wait_300:
        seat = seats[len(args.rounds)]
        require(seat_state(args.sql_server, database, seat) == ("Available", 0, 0, 0), "Ghế demo hết hạn đã được dùng.")
        started = time.monotonic()
        code, result = hold(sessions[0], trip, seat)
        require(code == 200, f"Giữ ghế để thử hết hạn trả HTTP {code}.")
        hold_id = str(UUID(json.loads(result)["id"]))
        seconds, expires = sql(args.sql_server, database, f"""
            SELECT DATEDIFF(second,CreatedUtc,ExpiresAtUtc), CONVERT(varchar(33),ExpiresAtUtc,126)
            FROM dbo.SeatHolds WHERE Id='{hold_id}';
        """)
        require(int(seconds) == 300, "SQL không lưu đúng hạn 300 giây.")
        print(f"Giữ 300 giây. Hết hạn UTC: {expires}. Đang chờ worker...", flush=True)
        while time.monotonic() - started < 330:
            if sql(args.sql_server, database, f"SELECT Status FROM dbo.SeatHolds WHERE Id='{hold_id}';") == ["Expired"]:
                break
            time.sleep(2)
        elapsed = time.monotonic() - started
        require(elapsed >= 299 and seat_state(args.sql_server, database, seat) == ("Available", 0, 0, 0),
                "Worker chưa nhả ghế đúng thời hạn.")
        code, _ = confirm(sessions[0], hold_id, seat)
        require(code == 409, f"Mã giữ hết hạn trả HTTP {code} thay vì 409.")
        code, result = hold(sessions[1], trip, seat)
        require(code == 200, f"Khách khác giữ lại ghế trả HTTP {code}.")
        release(sessions[1], str(UUID(json.loads(result)["id"])))
        evidence = {"hold_seconds_sql": int(seconds), "expires_utc_sql": expires,
                    "elapsed_seconds_observed": round(elapsed, 2), "expired_confirm_http": 409, "next_hold_http": 200}
        (output / "het-han.json").write_text(json.dumps(evidence, indent=2), encoding="utf-8")
        print(f"PASS worker nhả ghế sau khoảng {elapsed:.1f}s, mã cũ bị từ chối.", flush=True)
    return 0 if all_ok else 1


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    sys.stderr.reconfigure(encoding="utf-8")
    try:
        raise SystemExit(main())
    except (RuntimeError, OSError, ValueError, KeyError, subprocess.TimeoutExpired) as error:
        raise SystemExit(f"Lỗi demo: {error}") from error
