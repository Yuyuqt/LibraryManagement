using System;
using System.Collections.Generic;

namespace DbConnect.Entities;

public partial class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Isbn { get; set; } = null!;

    public string Author { get; set; } = null!;

    public string Status { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Description { get; set; }

    public string? CoverUrl { get; set; }

    public int TotalCopies { get; set; }

    public int AvailableCopies { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
