using BusBooking.Data;
using BusBooking.Services;
using BusBooking.Models;
using BusBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace BusBooking.Controllers;
public class AccountController(UserManager<AppUser> users,SignInManager<AppUser> signIn):Controller {
    [HttpGet] public async Task<IActionResult> Login(string? returnUrl=null) {
        if(User.Identity?.IsAuthenticated==true) {
            var current=await users.GetUserAsync(User);
            if(current!=null&&current.IsActive)return LocalRedirect(LoginDestination.Select(await users.GetRolesAsync(current),Url.IsLocalUrl(returnUrl)?returnUrl:null));
            await signIn.SignOutAsync();
        }
        return View(new LoginForm{ReturnUrl=returnUrl});
    }
    [HttpPost] public async Task<IActionResult> Login(LoginForm f) {
        if(!ModelState.IsValid){Response.StatusCode=400;return View(f);}
        var identifier=f.Email.Trim();
        var user=await users.FindByNameAsync(identifier)??await users.FindByEmailAsync(identifier);
        if(user!=null&&user.IsActive&&(await signIn.PasswordSignInAsync(user,f.Password,false,true)).Succeeded)
            return LocalRedirect(LoginDestination.Select(await users.GetRolesAsync(user),Url.IsLocalUrl(f.ReturnUrl)?f.ReturnUrl:null));
        ModelState.AddModelError("","Tên đăng nhập, email hoặc mật khẩu không đúng, hoặc tài khoản chưa hoạt động.");Response.StatusCode=400;return View(f);
    }
    [HttpGet] public IActionResult Register()=>View(new RegisterForm());
    [HttpPost] public async Task<IActionResult> Register(RegisterForm f) {
        if(!ModelState.IsValid){Response.StatusCode=400;return View(f);}
        var user=new AppUser{FullName=f.FullName.Trim(),Email=f.Email.Trim(),UserName=f.Email.Trim()};
        var result=await users.CreateAsync(user,f.Password);
        if(result.Succeeded) {
            var role=await users.AddToRoleAsync(user,Roles.Customer);
            if(!role.Succeeded){await users.DeleteAsync(user);ModelState.AddModelError("","Chưa có vai trò Customer. Hãy chạy seed development hoặc khởi tạo roles theo README.");Response.StatusCode=400;return View(f);}
            await signIn.SignInAsync(user,false);return RedirectToAction("Index","Home");
        }
        foreach(var e in result.Errors)ModelState.AddModelError("",e.Description);Response.StatusCode=400;return View(f);
    }
    [Authorize,HttpPost] public async Task<IActionResult> Logout(){await signIn.SignOutAsync();return RedirectToAction("Index","Home");}
    [Authorize,HttpGet]
    public IActionResult ChangePassword()=>View(new ChangePasswordForm());
    [Authorize,HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordForm form) {
        if(ModelState.IsValid) {
            var user=await users.GetUserAsync(User);
            if(user is null)return Challenge();
            var result=await users.ChangePasswordAsync(user,form.CurrentPassword,form.NewPassword);
            if(result.Succeeded) {
                await signIn.RefreshSignInAsync(user);
                TempData["Notice"]="Đã đổi mật khẩu.";
                return LocalRedirect(LoginDestination.Select(await users.GetRolesAsync(user),null));
            }
            foreach(var error in result.Errors) {
                if(error.Code==nameof(IdentityErrorDescriber.PasswordMismatch))
                    ModelState.AddModelError(nameof(form.CurrentPassword),"Mật khẩu hiện tại không đúng.");
                else ModelState.AddModelError("",error.Description);
            }
        }
        Response.StatusCode=400;
        return View(form);
    }
    [Authorize,HttpGet] public async Task<IActionResult> Profile() {
        var user=await users.GetUserAsync(User);
        if(user is null)return Challenge();
        var form=new ProfileForm {
            FullName=user.FullName,Phone=user.PhoneNumber,DateOfBirth=user.DateOfBirth,
            Address=user.Address,ProvinceCode=user.ProvinceCode
        };
        await ProfileMetadata(form,user);
        return View(form);
    }
    [Authorize,HttpPost] public async Task<IActionResult> Profile(ProfileForm f) {
        var user=await users.GetUserAsync(User);
        if(user is null)return Challenge();
        await ProfileMetadata(f,user);
        if(f.DateOfBirth>DateOnly.FromDateTime(BusinessTime.Vietnam(DateTime.UtcNow)))
            ModelState.AddModelError(nameof(f.DateOfBirth),"Ngày sinh không được ở tương lai.");
        if(!string.IsNullOrWhiteSpace(f.ProvinceCode)&&!ProvinceSearch.Provinces.ContainsKey(f.ProvinceCode))
            ModelState.AddModelError(nameof(f.ProvinceCode),"Vui lòng chọn tỉnh / thành phố hợp lệ.");
        if(ModelState.IsValid) {
            var phone=string.IsNullOrWhiteSpace(f.Phone)?null:f.Phone.Trim();
            if(user.PhoneNumber!=phone)user.PhoneNumberConfirmed=false;
            user.FullName=f.FullName.Trim();
            user.PhoneNumber=phone;
            user.DateOfBirth=f.DateOfBirth;
            user.Address=string.IsNullOrWhiteSpace(f.Address)?null:f.Address.Trim();
            user.ProvinceCode=string.IsNullOrWhiteSpace(f.ProvinceCode)?null:f.ProvinceCode;
            var result=await users.UpdateAsync(user);
            if(result.Succeeded) {
                TempData["Notice"]="Đã lưu hồ sơ.";
                return RedirectToAction(nameof(Profile));
            }
            foreach(var error in result.Errors)ModelState.AddModelError("",error.Description);
        }
        Response.StatusCode=400;
        return View(f);
    }
    private async Task ProfileMetadata(ProfileForm form,AppUser user) {
        form.UserId=user.Id;
        form.UserName=user.UserName??"";
        form.Email=user.Email??"";
        form.RoleLabel=string.Join(", ",(await users.GetRolesAsync(user)).Select(Roles.Label));
        form.IsActive=user.IsActive;
        form.CreatedUtc=user.CreatedUtc;
    }
}
