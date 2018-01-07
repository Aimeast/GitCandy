using GitCandy.Security;
using Microsoft.AspNetCore.Http;

namespace GitCandy.Accessories
{
    public class TokenAccessor : ITokenAccessor
    {
        private IHttpContextAccessor _httpContextAccessor;

        public TokenAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Token Token => _httpContextAccessor.HttpContext.Features.Get<Token>();
    }

    public interface ITokenAccessor
    {
        Token Token { get; }
    }
}
