using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeopleAccountsManager.Models
{

    [Table("Persons")]
    public class Person
    {
        [Key]
        [Column("code")]
        public int Code { get; set; }

        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        [Column("name", TypeName = "varchar(50)")]
        [Display(Name = "First Name")]
        public string? Name { get; set; }

        [StringLength(50, ErrorMessage = "Surname cannot exceed 50 characters")]
        [Column("surname", TypeName = "varchar(50)")]
        [Display(Name = "Last Name")]
        public string? Surname { get; set; }

        [Required(ErrorMessage = "ID Number is required")]
        [StringLength(50, ErrorMessage = "ID Number cannot exceed 50 characters")]
        [Column("id_number", TypeName = "varchar(50)")]
        [Display(Name = "ID Number")]
        public string IdNumber { get; set; } = string.Empty;

        public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

        [NotMapped]
        public string FullName => $"{Name ?? string.Empty} {Surname ?? string.Empty}".Trim();
    }
}