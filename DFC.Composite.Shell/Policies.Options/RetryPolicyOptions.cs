﻿namespace DFC.Composite.Shell.Policies.Options
{
    public class RetryPolicyOptions
    {
        public int Count { get; set; } = 3;

        public int BackoffPower { get; set; } = 2;

        public int BackOffBaseMilliseconds { get; set; } = 100;
    }
}
