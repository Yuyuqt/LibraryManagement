using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;

var handler = new JwtSecurityTokenHandler();
var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, "John Doe"),
    new Claim(ClaimTypes.Role, "Admin")
};

var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(claims)
};

var token = handler.CreateToken(tokenDescriptor);
var tokenString = handler.WriteToken(token);

Console.WriteLine("Token JSON payload:");
var jwt = handler.ReadJwtToken(tokenString);
foreach (var claim in jwt.Claims)
{
    Console.WriteLine($"{claim.Type}: {claim.Value}");
}

var identity = new ClaimsIdentity(jwt.Claims, "jwt");
Console.WriteLine($"Identity Name: {identity.Name ?? "NULL"}");
Console.WriteLine($"Identity NameClaimType: {identity.NameClaimType}");

var identity2 = new ClaimsIdentity(jwt.Claims, "jwt", "unique_name", "role");
Console.WriteLine($"Identity2 Name: {identity2.Name ?? "NULL"}");
