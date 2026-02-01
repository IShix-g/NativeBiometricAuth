#import <Foundation/Foundation.h>
#import <LocalAuthentication/LocalAuthentication.h>

#if TARGET_OS_OSX
static int EvaluateAuthentication(BOOL allowDeviceCredential)
{
    LAContext *context = [[LAContext alloc] init];
    if (!allowDeviceCredential) {
        context.localizedFallbackTitle = @"";
    }
    NSError *error = nil;
    LAPolicy policy = allowDeviceCredential
        ? LAPolicyDeviceOwnerAuthentication
        : LAPolicyDeviceOwnerAuthenticationWithBiometrics;
    if (![context canEvaluatePolicy:policy error:&error]) {
        if (!error) {
            return 0;
        }
        switch (error.code) {
            case LAErrorBiometryNotEnrolled:
            case LAErrorPasscodeNotSet:
                return 2;
            case LAErrorBiometryNotAvailable:
                return 1;
            default:
                return 0;
        }
    }

    __block BOOL success = NO;
    __block NSInteger failureReason = 4;
    dispatch_semaphore_t semaphore = dispatch_semaphore_create(0);

    void (^evaluateBlock)(void) = ^{
        [context evaluatePolicy:policy
                localizedReason:@"Authenticate to continue"
                          reply:^(BOOL didSucceed, NSError * _Nullable evalError) {
            success = didSucceed;
            if (!didSucceed)
            {
                if (evalError != NULL)
                {
                    switch (evalError.code)
                    {
                        case LAErrorUserCancel:
                        case LAErrorAppCancel:
                        case LAErrorSystemCancel:
                        case LAErrorUserFallback:
                            failureReason = 3;
                            break;
                        case LAErrorBiometryNotEnrolled:
                        case LAErrorPasscodeNotSet:
                            failureReason = 2;
                            break;
                        case LAErrorBiometryNotAvailable:
                            failureReason = 1;
                            break;
                        case LAErrorAuthenticationFailed:
                        case LAErrorBiometryLockout:
                        case LAErrorInvalidContext:
                            failureReason = 4;
                            break;
                        default:
                            failureReason = 5;
                            break;
                    }
                }
                else
                {
                    failureReason = 5;
                }
            }
            dispatch_semaphore_signal(semaphore);
        }];
    };

    if ([NSThread isMainThread]) {
        evaluateBlock();
        while (dispatch_semaphore_wait(semaphore, DISPATCH_TIME_NOW)) {
            [[NSRunLoop currentRunLoop] runMode:NSDefaultRunLoopMode
                                     beforeDate:[NSDate dateWithTimeIntervalSinceNow:0.01]];
        }
    } else {
        dispatch_async(dispatch_get_main_queue(), evaluateBlock);
        dispatch_semaphore_wait(semaphore, DISPATCH_TIME_FOREVER);
    }

    return success ? 0 : (int)failureReason;
}

extern "C" bool Authenticate(void)
{
    return EvaluateAuthentication(NO) == 0 ? true : false;
}

extern "C" bool AuthenticateWithOptions(bool allowDeviceCredential)
{
    return EvaluateAuthentication(allowDeviceCredential) == 0 ? true : false;
}

extern "C" int AuthenticateWithOptionsAndGetReason(bool allowDeviceCredential)
{
    return EvaluateAuthentication(allowDeviceCredential);
}

static int AvailabilityForPolicy(LAContext *context, LAPolicy policy)
{
    NSError *error = nil;
    if ([context canEvaluatePolicy:policy error:&error]) {
        return 0;
    }

    if (!error) {
        return 2;
    }

    switch (error.code) {
        case LAErrorBiometryNotEnrolled:
        case LAErrorPasscodeNotSet:
            return 1;
        case LAErrorBiometryNotAvailable:
            return 2;
        default:
            return 2;
    }
}

extern "C" int BiometricAvailability(void)
{
    LAContext *context = [[LAContext alloc] init];
    return AvailabilityForPolicy(context, LAPolicyDeviceOwnerAuthenticationWithBiometrics);
}

extern "C" int DeviceCredentialAvailability(void)
{
    LAContext *context = [[LAContext alloc] init];
    return AvailabilityForPolicy(context, LAPolicyDeviceOwnerAuthentication);
}
#endif
