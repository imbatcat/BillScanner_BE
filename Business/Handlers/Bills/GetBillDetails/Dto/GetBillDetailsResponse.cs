using Business.Handlers.Images.ProcessImage.Dto.ImageProcessing;
using Domain.Entities;

namespace Business.Handlers.Bills.GetBillDetails.Dto;

public record GetBillDetailsResponse(
    BillStatus Status,
    string ImgUrl,
    ImageProcessResult? ProcessResult,
    BillDto? Bill
);
