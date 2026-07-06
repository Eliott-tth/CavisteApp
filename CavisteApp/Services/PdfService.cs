using System;
using System.IO;
using CavisteApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CavisteApp.Services;

public class PdfService
{
    private readonly string _dossierTickets =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tickets");

    public string GenererTicket(Vente vente)
    {
        Directory.CreateDirectory(_dossierTickets);
        var cheminFichier = Path.Combine(_dossierTickets, $"ticket_{vente.Id}_{vente.DateVente:yyyyMMdd_HHmmss}.pdf");

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text("Caviste - Ticket de caisse").FontSize(18).Bold();
                    col.Item().Text($"Vente n°{vente.Id} - {vente.DateVente:dd/MM/yyyy HH:mm}");
                    col.Item().PaddingTop(5).Text($"Client : {vente.Client.NomComplet}");
                });

                page.Content().PaddingVertical(15).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Vin").Bold();
                        header.Cell().Text("Qté").Bold();
                        header.Cell().Text("P.U.").Bold();
                        header.Cell().Text("Total").Bold();
                    });

                    foreach (var ligne in vente.Lignes)
                    {
                        table.Cell().Text(ligne.Vin.Nom);
                        table.Cell().Text(ligne.Quantite.ToString());
                        table.Cell().Text($"{ligne.PrixUnitaire:N2} €");
                        table.Cell().Text($"{ligne.SousTotal:N2} €");
                    }
                });

                page.Footer().AlignRight().Text($"Montant total : {vente.MontantTotal:N2} €").FontSize(14).Bold();
            });
        }).GeneratePdf(cheminFichier);

        return cheminFichier;
    }
}
