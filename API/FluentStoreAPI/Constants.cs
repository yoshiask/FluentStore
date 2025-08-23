using System.Collections.Generic;
using Supabase.Gotrue.Exceptions;

namespace FluentStoreAPI;

public static class Constants
{
    public const string SUPABASE_URL = "https://obzzzkffllguthirhhku.supabase.co";
    public const string SUPABASE_KEY = "sb_publishable_I_uZ1ypjQOujpYbxf8T3Rw_dS1p1SN_";

    public static IReadOnlyDictionary<FailureHint.Reason, string> GotrueReasons { get; } = new Dictionary<FailureHint.Reason, string>
    {
        [FailureHint.Reason.Unknown] = "An unknown error occurred with the Gotrue auth service",
        [FailureHint.Reason.Offline] = "The client is offline",
        [FailureHint.Reason.UserEmailNotConfirmed] = "Please check your email for a confirmation link",
        [FailureHint.Reason.UserBadMultiple] = "Invalid email or password",
        [FailureHint.Reason.UserBadPassword] = "Invalid email or password",
        [FailureHint.Reason.UserBadLogin] = "Invalid email or password",
        [FailureHint.Reason.UserBadEmailAddress] = "Invalid email or password",
        [FailureHint.Reason.UserBadPhoneNumber] = "Invalid phone number",
        [FailureHint.Reason.UserMissingInformation] = "Missing required information",
        [FailureHint.Reason.UserAlreadyRegistered] = "A user with this email already exists",
        [FailureHint.Reason.UserTooManyRequests] = "You are being rate limited, please try again later",
        [FailureHint.Reason.MfaChallengeUnverified] = "Multi-factor authentication failed",
    };
}
