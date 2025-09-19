namespace Koop.Service.Services.TokenServices
{
    public interface ITokenSettings
    {
        string Audience { get; set; }
        string Issuer { get; set; }
        string Secret { get; set; }
        int TokenValidityMinutes { get; set; }
    }
}