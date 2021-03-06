﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Template10.Common
{
    /// <summary>
    /// Helper class for throttling events.
    /// </summary>
    public sealed class EventThrottleHelper : DependencyObject
    {
        /// <summary>
        /// Throttled event.
        /// </summary>
        public event EventHandler<object> ThrottledEvent;

        /// <summary>
        /// Throttle time in milliseconds.
        /// </summary>
        public int Throttle
        {
            get { return (int)GetValue(ThrottleProperty); }
            set { SetValue(ThrottleProperty, value); }
        }

        /// <summary>
        /// Reset throttle timer after each event.
        /// </summary>
        public bool ResetTimer
        {
            get { return (bool)GetValue(ResetTimerProperty); }
            set { SetValue(ResetTimerProperty, value); }
        }

        /// <summary>
        /// Reset throttle timer after each event.
        /// </summary>
        public static readonly DependencyProperty ResetTimerProperty = DependencyProperty.Register(nameof(ResetTimer), typeof(bool), typeof(EventThrottleHelper), new PropertyMetadata(false));


        /// <summary>
        /// Throttle time in milliseconds.
        /// </summary>
        public static readonly DependencyProperty ThrottleProperty = DependencyProperty.Register(nameof(Throttle), typeof(int), typeof(EventThrottleHelper), new PropertyMetadata(1000));

        /// <summary>
        /// Trigger a throttled event. Can only be called from the UI thread.
        /// </summary>
        /// <param name="eventData">Event data (only last event data will be transfered to event handler).</param>
        public async void TriggerEvent(object eventData)
        {
            // TODO: (Jerry) dispatch?
            await DelayAction(eventData).ConfigureAwait(false);
        }

        /// <summary>
        /// Trigger a throttled event inside a dispatcher.
        /// </summary>
        /// <param name="eventData">Event data (only last event data will be transfered to event handler).</param>
        public async void DispatchTriggerEvent(object eventData)
        {
            // TODO: (Jerry) dispatch?
            await DelayAction(eventData).ConfigureAwait(false);
        }

        private bool _isWaiting;

        private bool _isRefreshed;

        private DateTime _stamp;

        private object _savedEventData;

        private async Task DelayAction(object eventData)
        {
            Interlocked.Exchange(ref _savedEventData, eventData);
            if (ResetTimer || !_isWaiting)
            {
                _stamp = DateTime.Now;
                _isRefreshed = true;
            }
            if (_isWaiting)
            {
                return;
            }
            _isWaiting = true;
            try
            {
                while (_isRefreshed)
                {
                    _isRefreshed = false;
                    var toWait = (_stamp.AddMilliseconds(Throttle) - DateTime.Now);
                    if (toWait.Ticks > 0)
                    {
                        await Task.Delay(toWait).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                _isWaiting = false;
            }
            var callEventData = Interlocked.Exchange(ref _savedEventData, null);
            ThrottledEvent?.Invoke(this, callEventData);
        }
    }
}