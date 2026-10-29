namespace Riok.Mapperly.IntegrationTests.Dto
{
    public enum NoExpressionInliningProjectionStatusDto
    {
        Inactive = 10,
        Active = 20,
    }

    public class NoExpressionInliningProjectionDto
    {
        public int Id { get; set; }

        public NoExpressionInliningProjectionStatusDto Status { get; set; }
    }
}
