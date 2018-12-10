using Anotar.Serilog;
using System;
using System.Threading;

namespace SuperMemoAssistant
{
  /// <summary>
  /// Catches unhandled async exceptions
  /// https://github.com/Fody/AsyncErrorHandler
  /// </summary>
  public static class AsyncErrorHandler
  {
    public static void HandleException(Exception exception)
    {
      LogTo.Error(exception, "Unhandled async exception");

#if DEBUG
      SynchronizationContext.Current.Post(RethrowOnMainThread, exception);
#endif
    }

#if DEBUG
    private static void RethrowOnMainThread(object state)
    {
      Exception ex = state as Exception;

      // ReSharper disable once PossibleNullReferenceException
      throw ex;
    }
#endif
  }
}
