namespace Riok.Mapperly.IntegrationTests.Models
{
    public enum NoExpressionInliningProjectionStatus : byte
    {
        Active = 1,
        Inactive = 2,
    }

    public class NoExpressionInliningProjectionSource
    {
        public int Id { get; set; }

        public NoExpressionInliningProjectionStatus Status { get; set; }
    }
}
