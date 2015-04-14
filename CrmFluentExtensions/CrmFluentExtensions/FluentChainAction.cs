using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmFluentExtensions
{
    /// <summary>
    /// Fluent chain with no return type
    /// </summary>
    public class FluentChainAction
    {

        Action Work;
        Action<Action> chain;

        public FluentChainAction Retry()
        {
            return this.Combine((work) =>
            {
                RetryBase(1000, 1, (error) => DoNothing(error), DoNothing, work);
            });
        }

        public FluentChainAction Retry(int retryDuration, int retryCount)
        {
            return this.Combine((work) =>
            {
                RetryBase(retryDuration, retryCount, (error) => DoNothing(error), null, work);
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
        public FluentChainAction Retry(int retryDuration, int retryCount, Action<Exception> errorHandler, Action retryFailed)
        {
            return this.Combine((work) =>
            {
                RetryBase(retryDuration, retryCount, errorHandler, retryFailed, work);
            });
        }

        private void RetryBase(int retryDuration, int retryCount, Action<Exception> errorHandler, Action retryFailed, Action work)
        {
            do
            {
                try
                {
                    work();
                    return;
                }
                catch (Exception x)
                {
                    errorHandler(x);
                    System.Threading.Thread.Sleep(retryDuration);
                }
            } while (retryCount-- > 0);

            retryFailed();
        }

        /// <summary>
        /// It prevents the execution of the wapped action until the test is true
        /// </summary>
        /// <param name="test">Func return the test result</param>
        /// <returns></returns>
        public FluentChainAction Until(Func<bool> test)
        {
            return this.Combine(action =>
            {
                while (!test())
                { }
                action();
            });
        }

        /// <summary>
        /// Wraps the action in a while loop until the test fails
        /// </summary>
        /// <param name="test">Func for the test</param>
        /// <returns></returns>
        public FluentChainAction While(Func<bool> test)
        {
            return this.Combine(action =>
            {
                while (test())
                {
                    action();
                }

            });
        }

        /// <summary>
        /// Wraps the action in a while loop until the test fails
        /// </summary>
        /// <param name="test">Func for the test</param>
        /// <param name="completedOnce">Action to process called each loop execution</param>
        /// <returns></returns>
        public FluentChainAction While(Func<bool> test, Action completedOnce)
        {
            return this.Combine(action =>
            {
                while (test())
                {
                    action();
                    completedOnce();
                }
            });
        }

        /// <summary>
        /// Checks the condictions before execting the wrapped action.
        /// Throws <see cref="T:System.OperationCanceledException"/> when they are not all met
        /// </summary>
        /// <param name="conditions">list of conditions</param>
        /// <returns></returns>
        public FluentChainAction WhenTrue(params Func<bool>[] conditions)
        {
            return this.Combine(action =>
            {
                if (conditions.Any(condition => !condition()))
                    throw new OperationCanceledException("Conditions not met");
                else
                    action();
            });
        }

        /// <summary>
        /// Delays the action
        /// </summary>
        /// <param name="milliseconds">Milliseconds for the delay</param>
        /// <returns></returns>
        public FluentChainAction Delay(int milliseconds)
        {
            return this.Combine((action) =>
            {
                System.Threading.Thread.Sleep(milliseconds);
                action();
            });
        }


        /// <summary>
        /// Run the wapped methods asynchronously
        /// </summary>
        /// <param name="completeCallback">callback for completed execution</param>
        /// <returns></returns>
        public FluentChainAction RunAsync(Action completeCallback)
        {

            return this.Combine(work =>
            {
                work.BeginInvoke(asyncresult =>
                {
                    work.EndInvoke(asyncresult);
                    completeCallback();
                }, null);

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
        public FluentChainAction Log(Action<string> logger, string beforeMessage, string aftermessage)
        {
            return this.Combine((action) =>
            {
                logger(beforeMessage);
                action();
                logger(aftermessage);

            });
        }

        /// <summary>
        /// Meassures how long it took to execute the wrapped action
        /// </summary>
        /// <param name="logger"> action(string) to log the results </param>
        /// <param name="startMessage">starting message</param>
        /// <param name="endMessage">end message parameter {0} will contain <see cref="T:System.TimeSpan"/> difference</param>
        /// <returns></returns>
        public FluentChainAction HowLong(Action<string> logger, string startMessage, string endMessage)
        {
            return this.Combine((action) =>
            {
                logger(startMessage);
                var start = DateTime.Now;

                action();

                var finsih = DateTime.Now;
                var duration = finsih - start;

                logger(string.Format(endMessage, duration));

            });
        }

        /// <summary>
        /// Traps any exception on the wrapped action.
        /// Returns the indicated value on errors.
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public FluentChainAction TrapLog(Action<Exception> logger)
        {
            return this.Combine(action =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    logger(ex);
                }

            });
        }


        /// <summary>
        /// Traps any exception on the wrapped action.Logs it and throws it up
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        public FluentChainAction TrapLogThrow(Action<Exception> logger)
        {
            return this.Combine(action =>
            {
                try
                {
                    action();
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
        /// <returns></returns>
        public FluentChainAction Expected<E>() where E : Exception
        {
            return this.Combine(action =>
            {
                try
                {
                    action();
                }
                catch (E)
                {

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
        public FluentChainAction MustBeNonNull(params  object[] values)
        {
            return this.Combine((action) =>
            {
                for (var i = 0; i < values.Length; i++)
                {
                    var item = values[i];
                    if (item == null)
                    {
                        throw new ArgumentNullException(string.Format("Parameter at index {0} is null", i));
                    }
                }

                action();
            });
        }

        /// <summary>
        /// Validate that the parameters are not Null or deault 
        /// Throws <see cref="T:System.ArgumentException"/> for the first error
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public FluentChainAction MustBeNonDefault<T>(params T[] values)
        {
            return this.Combine((action) =>
            {
                for (var i = 0; i < values.Length; i++)
                {
                    var item = values[i];
                    if (item == null || item.Equals(default(T)))
                    {
                        throw new ArgumentNullException(string.Format("Parameter at index {0} is null or default", i));
                    }
                }

                action();
            });
        }


        #endregion

        #region Chain Methods
        /// <summary>
        /// Construction for the wrapper chain
        /// </summary>
        /// <param name="work">Final Operation to execute </param>
        public FluentChainAction(Action work)
        {
            this.Work = work;
        }

        /// <summary>
        /// Combines methods in the chain by wrapping them
        /// </summary>
        /// <param name="newDelegate">Wrapper</param>
        /// <returns></returns>
        public FluentChainAction Combine(Action<Action> newDelegate)
        {
            if (this.chain == null)
            {
                this.chain = newDelegate;
            }
            else
            {
                Action<Action> currentChain = this.chain;

                Action<Action> newChain = (action) =>
                {
                    currentChain(() => newDelegate(action));
                };

                this.chain = newChain;
            }

            return this;
        }

        /// <summary>
        /// Finalises the chain by execution the Action
        /// </summary>
        /// <returns></returns>
        public void Do()
        {
            if (this.chain == null)
            {
                Work();
            }
            else
            {
                this.chain(Work);
            }
        }

        private void DoNothing() { }

        private void DoNothing(params object[] whatever) { }

        private void DoNothing(Exception error) { }
        #endregion
    }
}
