using Business.Handlers.Bills.CreateBill.Dto;
using Business.Handlers.Images.ProcessImage.Dto.ImageProcessing;
using Domain.Entities;

namespace BillScanner.Models.Bills;

public record CreateBillModel
{
    public string ImgUrl { get; init; } = null!;
    public ExtractionMethod ExtractionMethod { get; init; }
    public UserEditsDto UserEdits { get; init; } = null!;
    public ImageProcessResult? RawExtraction { get; init; } = null!;
};