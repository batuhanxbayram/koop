using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

[NotMapped]
public class AppRole : IdentityRole<Guid>
{
}