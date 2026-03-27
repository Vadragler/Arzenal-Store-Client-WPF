using Arzenal.StoreManager.Core.Services;
using Arzenal.StoreManager.Core.Services.Helpers;
using HttpError = Arzenal.StoreManager.Core.Services.HttpError;

namespace Arzenal.StoreManager.Test.Unitaire
{
    public class HttpErrorServiceTests
    {
        [Fact]
        public void Unauthorized_SetsSessionDisconnected_AndRaisesEvent()
        {
            HttpError? received = null;
            void Handler(HttpError e) => received = e;

            HttpErrorService.OnError += Handler;

            try
            {
                AppSessionState.Instance.IsConnected = true;
                HttpErrorService.UnauthorizedAccess();

                Assert.False(AppSessionState.Instance.IsConnected);
                Assert.NotNull(received);
                Assert.Equal(401, received.Code);
            }
            finally
            {
                HttpErrorService.OnError -= Handler;
            }
        }

        [Fact]
        public void NotFound_RaisesEventWith404()
        {
            HttpError? received = null;
            void Handler(HttpError e) => received = e;
            HttpErrorService.OnError += Handler;
            try
            {
                HttpErrorService.NotFound("x");
                Assert.NotNull(received);
                Assert.Equal(404, received.Code);
            }
            finally { HttpErrorService.OnError -= Handler; }
        }
    }
}
