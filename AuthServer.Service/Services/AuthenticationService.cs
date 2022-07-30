using AuthServer.Core.Configrations;
using AuthServer.Core.Dtos;
using AuthServer.Core.Models;
using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedLibrary.Dtos;

namespace AuthServer.Service.Services
{
    public class AuthenticationService : IAuthenticationService
    {

        private readonly List<Client> _clients;
        private readonly ITokenService _tokenService;
        private readonly UserManager<UserApp> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<UserRefreshToken> _userRefreshTokenrepository;

        public AuthenticationService(IOptions<List<Client>> clients, ITokenService tokenService, UserManager<UserApp> userManager, IUnitOfWork unitOfWork, IGenericRepository<UserRefreshToken> repository)
        {
            _clients = clients.Value;
            _tokenService = tokenService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _userRefreshTokenrepository = repository;
        }

        public async Task<Response<TokenDto>> CreateTokenAsync(LoginDto loginDto)
        {
            if(loginDto == null) throw new ArgumentNullException("Login dto is not correct");

            var user=await _userManager.FindByEmailAsync(loginDto.Email);
            
            if(user == null) return Response<TokenDto>.Fail("Email or password is wrong",400,true);

            if(!(await _userManager.CheckPasswordAsync(user, loginDto.Password)))
                return Response<TokenDto>.Fail("Email or password is wrong", 400, true);

            var token = _tokenService.CreateToken(user);
            var userRefreshToken = await _userRefreshTokenrepository.Where(x => x.UserId == user.Id).SingleOrDefaultAsync();

            if (userRefreshToken != null)
            {
                await _userRefreshTokenrepository.AddAsync(new UserRefreshToken { UserId = user.Id, Code = token.RefreshToken, Expiration = token.RefreshTokenExpiration });
            }
            else
            {
                userRefreshToken.Code = token.RefreshToken;
                userRefreshToken.Expiration = token.RefreshTokenExpiration;
            }
            await _unitOfWork.CommitAsync();

            return Response<TokenDto>.Success(token, 200);

        }

        public Response<ClientTokenDto> CreateTokenByClient(ClientLoginDto clientLoginDto)
        {
            var client = _clients.SingleOrDefault(x => x.Id == clientLoginDto.ClientId && x.Secret == clientLoginDto.ClientSecret);

            if (client == null) return Response<ClientTokenDto>.Fail("Client id or secret not found", 404, true);

            var token = _tokenService.CreateTokenByClient(client);

            return Response<ClientTokenDto>.Success(token, 200);


        }

        public async Task<Response<TokenDto>> CreateTokenByRefreshToken(string refreshToken)
        {
            var rtoken = await _userRefreshTokenrepository.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();

            if(rtoken == null) return Response<TokenDto>.Fail("Refresh token not found",404, true);

            var user = await _userManager.FindByIdAsync(rtoken.UserId);

            if(user ==null) return Response<TokenDto>.Fail("User not found", 404, true);

            var tokenDto =_tokenService.CreateToken(user);

            rtoken.Code = tokenDto.RefreshToken;
            rtoken.Expiration = tokenDto.RefreshTokenExpiration;

            await _unitOfWork.CommitAsync();

            return Response<TokenDto>.Success(tokenDto, 200);

        }

        public async Task<Response<NoDataDto>> RevokeRefreshToken(string refreshToken)
        {
            var rtoken = await _userRefreshTokenrepository.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();
            
            if (rtoken == null) return Response<NoDataDto>.Fail("Refresh token not found", 404, true);

            _userRefreshTokenrepository.Remove(rtoken);

            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(204);
        }
    }
}
