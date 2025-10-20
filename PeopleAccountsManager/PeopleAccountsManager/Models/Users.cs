using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeopleAccountsManager.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        [Column("code")]
        public int Code { get; set; }

        [Required]
        [StringLength(50)]
        [Column("name", TypeName = "varchar(50)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("username", TypeName = "varchar(50)")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [Column("password_hash", TypeName = "varbinary(32)")]
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

        [Required]
        [Column("password_salt", TypeName = "varbinary(32)")]
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    }
}