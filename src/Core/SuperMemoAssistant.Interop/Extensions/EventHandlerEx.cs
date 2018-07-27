using System;
using System.Threading;
using System.Threading.Tasks;

namespace SuperMemoAssistant.Extensions
{
  public static class EventHandlerEx
  {
    public static Task<(bool success, object sender, TArgs args)> WaitEventAsync<TArgs>(this EventHandler<TArgs> eventHandler, int timeOut = Timeout.Infinite)
    {
      TArgs args = default(TArgs);
      object sender = null;
      AutoResetEvent ev = new AutoResetEvent(false);

      void callback(object _sender, TArgs _args)
      {
        args = _args;
        sender = _sender;
      };

      eventHandler += callback;

      return Task.Run(() =>
        {
          var ret = (ev.WaitOne(timeOut), sender, args);
          eventHandler -= callback;

          return ret;
        }
      );
    }
  }
}
