using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PeopleAccountsManager.Models
{

    [Table("Transactions")]
    public class Transaction
    {
 
        [Key]
        [Column("code")]
        public int Code { get; set; }

        [Required(ErrorMessage = "Account is required")]
        [Column("account_code")]
        [Display(Name = "Account")]
        public int AccountCode { get; set; }

        [Required(ErrorMessage = "Transaction Date is required")]
        [DataType(DataType.Date)]
        [Column("transaction_date")]
        [Display(Name = "Transaction Date")]
        public DateTime TransactionDate { get; set; }

        [Required]
        [Column("capture_date")]
        [Display(Name = "Capture Date")]
        public DateTime CaptureDate { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Column("amount", TypeName = "money")]
        [Display(Name = "Amount")]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters")]
        [Column("description", TypeName = "varchar(100)")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [ForeignKey("AccountCode")]
        public virtual Account? Account { get; set; }
        [NotMapped]
        public bool IsValidAmount => Amount != 0;
    }
}