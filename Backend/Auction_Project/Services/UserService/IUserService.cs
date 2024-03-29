﻿using Auction_Project.Models.Users;
using System.IdentityModel.Tokens.Jwt;

namespace Auction_Project.Services.UserService;

public interface IUserService
{
    Task<string?> VeryfyData(UserRegisterDTO user);

    Task<bool> CheckPassword(UserLoginDTO user);

    Task<JwtSecurityToken> GenerateToken(UserLoginDTO user);

    Task<UserResponseDTO?> AddUser(UserRegisterDTO model);

    Task<bool> ChangePassword(UserChangePasswordDTO dto);
    Task<bool> isUserBanned(string username);

    List<UserResponseDTO> GetAll();
    Task<bool> ChangeUserRole(UserRoleDTO role);
    string GetMyName();

    string GetMyRole();

    bool IsValidCNP(string cnp);

    bool IsValidEmail(string email);

    int AgeFromCnp(string cnp);
    Task<User> GetMe();
    List<UserResponseDTO> GetAllClientsByPage(int nr);
    List<UserResponseDTO> GetAllClients();
    Task<UserResponseDTO> BanUser(string id);
    Task<string> GetMyId();
    Task<UserResponseDTO> GetMeDTO();
    Task<UserResponseDTO> GetUserById(string userName);
    Task<List<string>> GetUserRolesById(string userName);
}
