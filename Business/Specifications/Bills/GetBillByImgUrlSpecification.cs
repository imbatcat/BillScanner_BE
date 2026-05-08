using Domain.Entities;

namespace Business.Specifications.Bills
{
    public class GetBillByImgUrlSpecification : BaseSpecification<Bill>
    {
        public GetBillByImgUrlSpecification(string imgUrl)
            : base(b => b.ImgUrl == imgUrl) { }
    }
}
