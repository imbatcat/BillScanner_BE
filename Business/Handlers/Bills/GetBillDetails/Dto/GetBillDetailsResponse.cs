using Business.Handlers.Images.ProcessImage.Dto.ImageProcessing;
using Domain.Entities;

namespace Business.Handlers.Bills.GetBillDetails.Dto;

public record GetBillDetailsResponse(
    string ImgUrl,
    ImageProcessResult? ProcessResult,
    BillDto? Bill
);
