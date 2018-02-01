namespace AcmeCorp.EventSourcing.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using Xunit;

    public static class AssertExtensions
    {
        public static async Task ThrowsAsync<T>(Func<Task> testDelegate)
        {
            await Task.Run(async () =>
            {
                try
                {
                    await testDelegate().ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    Assert.Equal(typeof(T), exception.GetType());
                }
            }).ConfigureAwait(false);
        }
    }
}