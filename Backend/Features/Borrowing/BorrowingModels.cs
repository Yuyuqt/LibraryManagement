using System;

namespace Backend.Features.Borrowings
{
    public class BorrowingDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal FineAmount { get; set; }
        public bool IsFinePaid { get; set; }
    }

    public class BorrowRequest
    {
        public int BookId { get; set; }
    }
}
