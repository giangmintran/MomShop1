﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MOMShop.Domain.Interfaces;
using MOMShop.Dto.Product;
using MOMShop.Dto.Users;
using MOMShop.Entites;
using MOMShop.MomShopDbContext;
using MOMShop.Utils;
using MOMShop.Utils.Validations;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using ClaimTypes = MOMShop.Utils.ConstantVariables.ClaimTypes;
using SymmetricSecurityKey = Microsoft.IdentityModel.Tokens.SymmetricSecurityKey;

namespace MOMShop.Services.Implements
{
    public class UsersServices : IUsersServices
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        private readonly ApplicationDbContext _dbContext;
        //private readonly AuthOtpRepository _authOtpRepository;

        public UsersServices(IConfiguration configuration,
            IMapper mapper,
            ApplicationDbContext dbContext)
        {
            _configuration = configuration;
            _mapper = mapper;
            _dbContext = dbContext;
            //_authOtpRepository = new AuthOtpRepository(_dbContext, _logger);
        }

        public static List<Users> test = new List<Users>();

        //public UsersDto FindByUsername(string username)
        //{
        //    var user = test.FirstOrDefault(u => u.Username == username);
        //    var result = _mapper.Map<UsersDto>(user);
        //    return result;
        //}

        //public UsersDto FindMyInfo()
        //{
        //    var userId = CommonUtils.GetCurrentUserId(_httpContext);
        //    var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
        //    var result = _mapper.Map<UsersDto>(user);
        //    return result;
        //}


        public void Create(CreateUsersDto input)
        {
            var checkUsername = _dbContext.Users.Any(u => u.Username == input.Username);
            if (checkUsername)
            {
                throw new Exception($"Tài khoản người dùng đã tồi tại");
            }
            var userInsert = _dbContext.Users.Add(new Users
            {
                Username = input.Username,
                Password = CommonUtils.CreateMD5(input.Password),
                Phone = "",
                //
            }).Entity;
            _dbContext.SaveChanges();

            //var walletInsert = _dbContext.Wallets.Add(new Wallet
            //{
            //    UserId = userInsert.Id,
            //    //Logo = input.Logo,
            //    Type = WalletTypes.ELECTRONIC,
            //    Status = StatusString.ACTIVE,
            //    IsDefault = YesNo.YES,
            //    CreatedBy = userInsert.Username,
            //    CreatedDate = DateTime.Now,
            //});
            //_dbContext.SaveChanges();
            //var wallet2Insert = _dbContext.Wallets.Add(new Wallet
            //{
            //    UserId = userInsert.Id,
            //    //Logo = input.Logo,
            //    Type = WalletTypes.SAVING,
            //    Status = StatusString.ACTIVE,
            //    IsDefault = YesNo.NO,
            //    CreatedBy = userInsert.Username,
            //    CreatedDate = DateTime.Now,
            //});
            //_dbContext.SaveChanges();
        }

        //public AuthOtpDto AddOtp(int type)
        //{
        //    var userId = CommonUtils.GetCurrentUserId(_httpContext);
        //    var phone = CommonUtils.GetCurrentPhone(_httpContext);
        //    var otp = _authOtpRepository.Add(userId, phone, type);
        //    _dbContext.SaveChanges();
        //    var result = new AuthOtpDto();
        //    result.Otp = otp.OtpCode;
        //    return result;
        //}

        //public void AuthOtpPhone(string otp)
        //{
        //    var userId = CommonUtils.GetCurrentUserId(_httpContext);
        //    _authOtpRepository.AuthOtp(otp, userId, AuthOtpTypes.AUTH_PHONE);
        //    var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
        //    if (user != null)
        //    {
        //        user.IsIdentifier = YesNo.YES;
        //    }
        //    _dbContext.SaveChanges();
        //}
        //public void CreateInfoUser(CreateUserInfoDto input)
        //{
        //    var userId = CommonUtils.GetCurrentUserId(_httpContext);

        //    var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
        //    if (user != null)
        //    {
        //        user.FullName = input.FullName;
        //        user.Phone = input.Phone;
        //        user.Sex = input.Sex;
        //        user.Email = input.Email;
        //    }
        //    _dbContext.SaveChanges();
        //}

        public LoginResultDto Login(LoginDto input)
        {
            var user = _dbContext.Users.FirstOrDefault(u => u.Username == input.Username);
            if (user == null)
            {
                throw new Exception($"Tài khoản hoặc mật khẩu không chính xác");
            }

            if (CommonUtils.CreateMD5(input.Password) == user.Password)
            {
                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(ClaimTypes.Username, user.Username),
                    //new Claim(CustomClaimTypes.UserType, user.UserType.ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddSeconds(_configuration.GetValue<int>("JWT:Expires")),
                    claims: claims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

                var userData = _mapper.Map<UsersDto>(user);
                //var walletUserFind = _dbContext.Wallets.FirstOrDefault(w => w.UserId == user.Id && w.Type == WalletTypes.ELECTRONIC
                //                        && w.IsDefault == YesNo.YES);
                //if (walletUserFind != null)
                //{
                //    userData.AmountMoney = walletUserFind.AmountMoney;
                //    userData.Logo = walletUserFind.Logo;
                //}
                return new LoginResultDto()
                {
                    access_token = new JwtSecurityTokenHandler().WriteToken(token),
                    expires_in = 7200,
                    refresh_token = "E34D9D799DFCCBF92783FD866F560893CC0A333C3AF9675B28012537FE008D15",
                    scope = "API offline_access",
                    token_type = "Bearer",
                    UserData = userData,
                };
            }
            else
            {
                throw new Exception("Tài khoản hoặc mật khẩu không chính xác");
            }
        }

        //public UsersDto UpdateAvatar(UpdateAvatarDto input)
        //{
        //    var userId = CommonUtils.GetCurrentUserId(_httpContext);
        //    var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
        //    if (user != null)
        //    {
        //        user.Avatar = input.Avatar;
        //    }
        //    _dbContext.SaveChanges();
        //    return _mapper.Map<UsersDto>(user);
        //}
    }
}
