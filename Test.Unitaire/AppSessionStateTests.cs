using Arzenal.StoreManager.Core.Services.Helpers;
using System.ComponentModel;
using Xunit;

namespace Arzenal.StoreManager.Test.Unitaire
{
    public class AppSessionStateTests
    {
        [Fact]
        public void IsConnected_PropertyChangedFires()
        {
            var instance = AppSessionState.Instance;
            bool fired = false;
            instance.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(AppSessionState.IsConnected)) fired = true; };

            instance.IsConnected = !instance.IsConnected;
            Assert.True(fired);
        }
    }
}
