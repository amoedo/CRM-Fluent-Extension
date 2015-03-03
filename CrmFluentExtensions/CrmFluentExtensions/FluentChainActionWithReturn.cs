using CrmFluentExtensions.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmFluentExtensions
{
    /// <summary>
    /// Fluent chain with return Type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FluentChainActionWithReturn<T>
    {

        Func<T> Work;
        Func<Func<T>, T> chain;

        /// <summary>
        /// Retries the operation once with a 1s delay
        /// </summary>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> Retry()
        {
            return this.Combine((work) =>
            {
                return RetryBase(1000, 1, (error) => DoNothing(error), DoNothing, work);
            });
        }

        /// <summary>
        /// Retries the operation the indicated number of times with a
        /// </summary>
        /// <param name="retryDuration"></param>
        /// <param name="retryCount"></param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> Retry(int retryDuration, int retryCount)
        {
            return this.Combine((work) =>
            {
                return RetryBase(retryDuration, retryCount, (error) => DoNothing(error), DoNothing, work);
            });
        }

        /// <summary>
        /// Retries the wrapped operation
        /// </summary>
        /// <param name="retryDuration"></param>
        /// <param name="retryCount"></param>
        /// <param name="errorHandler"></param>
        /// <param name="retryFailed"></param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> Retry(int retryDuration, int retryCount, Action<Exception> errorHandler, Func<T> retryFailed)
        {
            return this.Combine((work) =>
            {
                return RetryBase(retryDuration, retryCount, errorHandler, retryFailed, work);
            });
        }

        private T RetryBase(int retryDuration, int retryCount, Action<Exception> errorHandler, Func<T> retryFailed, Func<T> work)
        {
            do
            {
                try
                {
                    return work();
                }
                catch (Exception x)
                {
                    errorHandler(x);
                    System.Threading.Thread.Sleep(retryDuration);
                }
            } while (retryCount-- > 0);

            return retryFailed();
        }

        /// <summary>
        /// It prevents the execution of the wapped action until the test is true
        /// </summary>
        /// <param name="test">Func return the test result</param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> Until(Func<bool> test)
        {
            return this.Combine(action =>
            {
                while (!test())
                { }
                return action();
            });
        }

        /// <summary>
        /// Wraps the action in a while loop until the test fails
        /// </summary>
        /// <param name="test">Func for the test</param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> While(Func<bool> test)
        {
            return this.Combine(action =>
            {
                T lastResult = default(T);

                while (test())
                {
                    lastResult = action();
                }

                return lastResult;
            });
        }

        /// <summary>
        /// Wraps the action in a while loop until the test fails
        /// </summary>
        /// <param name="test">Func for the test</param>
        /// <param name="newValueCreated">Action to process each value of type T obtained</param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> While(Func<bool> test, Action<T> newValueCreated)
        {
            return this.Combine(action =>
            {
                T lastResult = default(T);

                while (test())
                {
                    lastResult = action();
                    newValueCreated(lastResult);
                }

                return lastResult;
            });
        }

        /// <summary>
        /// Checks the condictions before execting the wrapped action.
        /// Throws <see cref="T:System.OperationCanceledException"/> when they are not all met
        /// </summary>
        /// <param name="conditions">list of conditions</param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> WhenTrue(params Func<bool>[] conditions)
        {
            return this.Combine(action =>
            {
                if (conditions.Any(condition => !condition()))
                    throw new OperationCanceledException("Conditions not met");
                return action();
            });
        }

        /// <summary>
        /// Delays the action
        /// </summary>
        /// <param name="milliseconds">Milliseconds for the delay</param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> Delay(int milliseconds)
        {
            return this.Combine((action) =>
            {
                System.Threading.Thread.Sleep(milliseconds);
                return action();
            });
        }

        /// <summary>
        /// Run the wapped methods asynchronously return default(T). Actual value is passed to the callback
        /// </summary>
        /// <param name="completeCallback">Callback for execution completed</param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> RunAsync(Action<T> completeCallback)
        {
            return this.RunAsync(completeCallback, default(T));
        }

        /// <summary>
        /// Run the wapped methods asynchronously returning fakevalue. Actual value is passed to the callback
        /// </summary>
        /// <param name="completeCallback">callback for completed execution</param>
        /// <param name="fakeReturn">fake return value</param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> RunAsync(Action<T> completeCallback, T fakeReturn)
        {

            return this.Combine(work =>
            {
                work.BeginInvoke(asyncresult =>
                {
                    var result = work.EndInvoke(asyncresult);
                    completeCallback(result);
                }, null);

                return fakeReturn;
            });
        }

        /// <summary>
        /// Checks the cache for an existing result for the key and returns that value instead.
        /// If the cache value is not present or expired the execution continues and the result is saved 
        /// for future calls using the Time to Live specified by default on the <see cref="T:CrmFluentExtensions.Data.DataCache"/>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> Cache(string key)
        {
            return this.Combine(work =>
            {
                if (DataCache.HasValue(key))
                {
                    return DataCache.GetValue<T>(key);
                }
                else
                {
                    T result = work();
                    DataCache.SetValue(key, result);
                    return result;
                }
            });
        }

        /// <summary>
        /// Checks the cache for an existing result for the key and returns that value instead.
        /// If the cache value is not present or expired the execution continues and the result is saved 
        /// for future calls using the Time to Live specified
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> Cache(string key, TimeSpan timeToLeave)
        {
            return this.Combine(work =>
                {
                    if (DataCache.HasValue(key))
                    {
                        return DataCache.GetValue<T>(key);
                    }
                    else
                    {
                        T result = work();
                        DataCache.SetValue(key, result, timeToLeave);
                        return result;
                    }
                });
        }

        #region Log and Exception Wrappers
        /// <summary>
        /// Write to the logger before and after the wrapped operation
        /// </summary>
        /// <param name="logger">action(string) logger function</param>
        /// <param name="beforeMessage"></param>
        /// <param name="aftermessage"></param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> Log(Action<string> logger, string beforeMessage, string aftermessage)
        {
            return this.Combine((action) =>
            {
                logger(beforeMessage);
                T result = action();
                logger(aftermessage);
                return result;
            });
        }

        /// <summary>
        /// Meassures how long it took to execute the wrapped action
        /// </summary>
        /// <param name="logger"> action(string) to log the results </param>
        /// <param name="startMessage">starting message</param>
        /// <param name="endMessage">end message parameter {0} will contain <see cref="T:System.TimeSpan"/> difference</param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> HowLong(Action<string> logger, string startMessage, string endMessage)
        {
            return this.Combine((action) =>
            {
                logger(startMessage);
                var start = DateTime.Now;

                T result = action();

                var finsih = DateTime.Now;
                var duration = finsih - start;

                logger(string.Format(endMessage, duration));

                return result;
            });
        }

        /// <summary>
        /// Traps any exception on the wrapped action.
        /// Returns the indicated value on errors.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="returnInError"></param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> TrapLog(Action<Exception> logger, T returnInError)
        {
            return this.Combine(action =>
            {
                try
                {
                    return action();
                }
                catch (Exception ex)
                {
                    logger(ex);
                }

                return returnInError;
            });
        }

        /// <summary>
        /// Traps any exception on the wrapped action.
        /// Returns the default(T) value in errors
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> TrapLog(Action<Exception> logger)
        {
            return this.TrapLog(logger, default(T));
        }

        /// <summary>
        /// Traps any exception on the wrapped action.Logs it and throws it up
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> TrapLogThrow(Action<Exception> logger)
        {
            return this.Combine(action =>
            {
                try
                {
                    return action();
                }
                catch (Exception ex)
                {
                    logger(ex);
                    throw;
                }
            });
        }

        /// <summary>
        /// Traps the Exception type and returns the indicated value instead
        /// </summary>
        /// <param name="valueToReturn">value to return on error</param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> Expected<E>(T valueToReturn) where E : Exception
        {
            return this.Combine(action =>
            {
                try
                {
                    return action();
                }
                catch (E)
                {
                    return valueToReturn;
                }
            });
        }

        #endregion

        #region Parameter and Return Wrappers

        /// <summary>
        /// Validate that the parameters are not Null. 
        /// Throws <see cref="T:System.ArgumentNullException"/> for the first error
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> MustBeNonNull(params  object[] values)
        {
            return this.Combine((action) =>
            {
                for (var i = 0; i < values.Length; i++)
                {
                    var item = values[i];
                    if (item == null) throw new ArgumentNullException(
                        string.Format("Parameter of Type {0} at index {1} is null", item.GetType(), i)
                        );
                }

                return action();
            });
        }

        /// <summary>
        /// Validate that the parameters are not Null or deault 
        /// Throws <see cref="T:System.ArgumentException"/> for the first error
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> MustBeNonDefault(params T[] values)
        {
            return this.Combine((action) =>
            {
                for (var i = 0; i < values.Length; i++)
                {
                    var item = values[i];
                    if (item == null || item.Equals(default(T)))
                    {
                        throw new ArgumentNullException(
                        string.Format("Parameter of Type {0} at index {1} is null or default", item.GetType(), i)
                        );
                    }

                }

                return action();
            });
        }

        /// <summary>
        /// Validates that the result of the wrapped operation is not Null or Defaul
        /// Throws <see cref="T:System.InvalidOperationException"/> 
        /// </summary>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> ReturnMustBeNonNullorDefault()
        {
            return this.Combine((action) =>
            {
                T result = action();

                if (result == null || result.Equals(default(T)))
                {
                    throw new InvalidOperationException("Action result is null or default");
                }

                return result;
            });

        }

        #endregion

        #region Chain Methods
        /// <summary>
        /// Construction for the wrapper chain
        /// </summary>
        /// <param name="work">Final Operation to execute return value</param>
        public FluentChainActionWithReturn(Func<T> work)
        {
            this.Work = work;
        }

        /// <summary>
        /// Combines methods in the chain by wrapping them
        /// </summary>
        /// <param name="newDelegate">Wrapper</param>
        /// <returns></returns>
        public FluentChainActionWithReturn<T> Combine(Func<Func<T>, T> newDelegate)
        {
            if (this.chain == null)
            {
                this.chain = newDelegate;
            }
            else
            {
                Func<Func<T>, T> currentChain = this.chain;

                Func<Func<T>, T> newChain = (action) =>
                {
                    return currentChain(() => newDelegate(action));
                };

                this.chain = newChain;
            }

            return this;
        }

        /// <summary>
        /// Finalises the chain by execution the base function
        /// </summary>
        /// <returns></returns>
        public T Do()
        {
            if (this.chain == null)
            {
                return Work();
            }
            else
            {
                return this.chain(Work);
            }
        }

        private T DoNothing() { return default(T); }

        private void DoNothing(Exception error) { }
        #endregion
    }
}
