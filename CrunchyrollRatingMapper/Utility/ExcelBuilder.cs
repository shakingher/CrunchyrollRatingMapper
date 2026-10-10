using ClosedXML.Excel;
using CrunchyrollRatingMapper.DTOs;

namespace CrunchyrollRatingMapper.Utility;
public static class ExcelBuilder
{
    public static void BuildExcel(List<AnimeInfo> currentAnimes)
    {
        string outputDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "Exports");

        Directory.CreateDirectory(outputDirectory);

        string filePath = Path.Combine(
            outputDirectory,
            $"Animes_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx");

        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Anime");

        string[] headers =
        {
            "CrunchyRoll Id",
            "Title",
            "Genres",
            "Availability",
            "Average Rating",
            "Total Ratings",
            "MAL Id",
            "MAL Rating",
            "MAL Genres"
        };

        for (int col = 0; col < headers.Length; col++)
        {
            worksheet.Cell(1, col + 1).Value = headers[col];
        }

        worksheet.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;

        int row = 2;

        foreach (var anime in currentAnimes)
        {
            worksheet.Cell(row, 1).Value = anime.Id;
            worksheet.Cell(row, 2).Value = anime.Title;
            worksheet.Cell(row, 3).Value = anime.Genres;
            worksheet.Cell(row, 4).Value = anime.Availability;

            if (anime.AverageRating.HasValue)
                worksheet.Cell(row, 5).Value = anime.AverageRating.Value;

            if (anime.TotalRatings.HasValue)
                worksheet.Cell(row, 6).Value = anime.TotalRatings.Value;

            worksheet.Cell(row, 7).Value = anime.MalId;

            if (anime.MalRating.HasValue)
                worksheet.Cell(row, 8).Value = anime.MalRating.Value;

            worksheet.Cell(row, 9).Value = anime.MalGenres;

            row++;
        }

        worksheet.SheetView.FreezeRows(1);
        worksheet.RangeUsed()?.SetAutoFilter();
        worksheet.Columns().AdjustToContents();

        workbook.SaveAs(filePath);

        Console.WriteLine($"Excel file saved: {filePath}");
        Console.WriteLine($"Anime exported: {currentAnimes.Count}");
    }
}
