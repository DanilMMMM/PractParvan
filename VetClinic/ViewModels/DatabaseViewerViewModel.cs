namespace VetClinic.ViewModels;

public class DatabaseViewerViewModel
{
    public string DbPath { get; set; } = string.Empty;
    public string DbFileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime LastModified { get; set; }
    public string SelectedTable { get; set; } = string.Empty;
    public List<TableSummary> Tables { get; set; } = new();
    public List<string> ColumnNames { get; set; } = new();
    public List<List<string>> Rows { get; set; } = new();
    public string? CustomSql { get; set; }
    public string? SqlError { get; set; }
    public int TotalRows { get; set; }
}

public class TableSummary
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int RowCount { get; set; }
}
