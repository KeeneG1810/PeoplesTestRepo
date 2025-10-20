using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeopleAccountsManager.Models
{

    [Table("Accounts")]
    public class Account
    {

        [Key]
        [Column("code")]
        public int Code { get; set; }

        [Required(ErrorMessage = "Person is required")]
        [Column("person_code")]
        [Display(Name = "Person")]
        public int PersonCode { get; set; }

        [Required(ErrorMessage = "Account Number is required")]
        [StringLength(50, ErrorMessage = "Account Number cannot exceed 50 characters")]
        [Column("account_number", TypeName = "varchar(50)")]
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; } = string.Empty;


        [Column("outstanding_balance", TypeName = "money")]
        [Display(Name = "Outstanding Balance")]
        [DataType(DataType.Currency)]
        public decimal OutstandingBalance { get; set; }

        [NotMapped]
        [Display(Name = "Account Closed")]
        public bool IsClosed { get; set; } = false;

  
        [ForeignKey("PersonCode")]
        public virtual Person? Person { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}