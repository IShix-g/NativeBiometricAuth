#import <UIKit/UIKit.h>
#import <CoreImage/CoreImage.h>
#import <QuartzCore/QuartzCore.h>
#import <TargetConditionals.h>

#if TARGET_OS_IOS || TARGET_OS_TV
static bool s_privacyEnabled = false;
static UIView *s_privacyView = nil;
static CIContext *s_ciContext = nil;

static UIWindow *PrivacyWindowFromScenes(void)
{
    if (@available(iOS 13.0, *))
    {
        NSSet<UIScene *> *scenes = [UIApplication sharedApplication].connectedScenes;
        for (UIScene *scene in scenes)
        {
            if (![scene isKindOfClass:[UIWindowScene class]])
            {
                continue;
            }
            UIWindowScene *windowScene = (UIWindowScene *)scene;
            if (windowScene.activationState != UISceneActivationStateForegroundActive
                && windowScene.activationState != UISceneActivationStateForegroundInactive)
            {
                continue;
            }
            for (UIWindow *window in windowScene.windows)
            {
                if (window.isKeyWindow)
                {
                    return window;
                }
            }
            if (windowScene.windows.count > 0)
            {
                return windowScene.windows.firstObject;
            }
        }
    }
    return nil;
}

static UIWindow *PrivacyActiveWindow(void)
{
    UIWindow *sceneWindow = PrivacyWindowFromScenes();
    if (sceneWindow != nil)
    {
        return sceneWindow;
    }
    UIWindow *window = [UIApplication sharedApplication].keyWindow;
    if (window != nil)
    {
        return window;
    }
    for (UIWindow *candidate in [UIApplication sharedApplication].windows)
    {
        if (candidate.isKeyWindow)
        {
            return candidate;
        }
    }
    return [UIApplication sharedApplication].windows.firstObject;
}

static void PrivacyEnsureView(void)
{
    if (s_privacyView != nil)
    {
        return;
    }
    UIImageView *view = [[UIImageView alloc] initWithFrame:CGRectZero];
    view.autoresizingMask = UIViewAutoresizingFlexibleWidth | UIViewAutoresizingFlexibleHeight;
    view.userInteractionEnabled = NO;
    view.contentMode = UIViewContentModeScaleToFill;
    view.backgroundColor = [UIColor blackColor];
    s_privacyView = view;
    if (s_ciContext == nil)
    {
        s_ciContext = [CIContext contextWithOptions:nil];
    }
}

static UIImage *PrivacyCaptureImage(UIWindow *window)
{
    if (window == nil)
    {
        return nil;
    }
    CGSize size = window.bounds.size;
    if (size.width <= 0 || size.height <= 0)
    {
        return nil;
    }
    UIGraphicsBeginImageContextWithOptions(size, YES, 0.0);
    BOOL ok = [window drawViewHierarchyInRect:window.bounds afterScreenUpdates:NO];
    if (!ok)
    {
        [window.layer renderInContext:UIGraphicsGetCurrentContext()];
    }
    UIImage *image = UIGraphicsGetImageFromCurrentImageContext();
    UIGraphicsEndImageContext();
    return image;
}

static UIImage *PrivacyBlurImage(UIImage *image)
{
    if (image == nil)
    {
        return nil;
    }
    CIImage *input = [[CIImage alloc] initWithImage:image];
    if (input == nil)
    {
        return nil;
    }
    CIFilter *clamp = [CIFilter filterWithName:@"CIAffineClamp"];
    [clamp setValue:input forKey:kCIInputImageKey];
    [clamp setValue:[NSValue valueWithCGAffineTransform:CGAffineTransformIdentity] forKey:@"inputTransform"];
    CIImage *clamped = clamp.outputImage;
    CIFilter *blur = [CIFilter filterWithName:@"CIGaussianBlur"];
    [blur setValue:clamped forKey:kCIInputImageKey];
    [blur setValue:@(24.0) forKey:kCIInputRadiusKey];
    CIImage *output = blur.outputImage;
    if (output == nil || s_ciContext == nil)
    {
        return nil;
    }
    CGRect extent = [input extent];
    CGImageRef cgImage = [s_ciContext createCGImage:output fromRect:extent];
    if (cgImage == nil)
    {
        return nil;
    }
    UIImage *result = [UIImage imageWithCGImage:cgImage scale:image.scale orientation:image.imageOrientation];
    CGImageRelease(cgImage);
    return result;
}

static void PrivacyShowIfNeeded(void)
{
    if (!s_privacyEnabled)
    {
        return;
    }
    PrivacyEnsureView();
    UIWindow *window = PrivacyActiveWindow();
    if (window == nil)
    {
        return;
    }
    s_privacyView.frame = window.bounds;
    if ([s_privacyView isKindOfClass:[UIImageView class]])
    {
        UIImage *snapshot = PrivacyCaptureImage(window);
        UIImage *blurred = PrivacyBlurImage(snapshot);
        UIImageView *imageView = (UIImageView *)s_privacyView;
        if (blurred != nil)
        {
            imageView.image = blurred;
            imageView.backgroundColor = [UIColor clearColor];
        }
        else
        {
            imageView.image = nil;
            imageView.backgroundColor = [UIColor blackColor];
        }
    }
    if (s_privacyView.superview != window)
    {
        [window addSubview:s_privacyView];
    }
}

static void PrivacyHide(void)
{
    [s_privacyView removeFromSuperview];
}

static void PrivacyWillResignActive(NSNotification *notification)
{
    PrivacyShowIfNeeded();
}

static void PrivacyDidBecomeActive(NSNotification *notification)
{
    PrivacyHide();
}

static void PrivacyInstallObservers(void)
{
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        NSNotificationCenter *center = [NSNotificationCenter defaultCenter];
        [center addObserverForName:UIApplicationWillResignActiveNotification
                            object:nil
                             queue:[NSOperationQueue mainQueue]
                        usingBlock:^(NSNotification *note) {
            PrivacyWillResignActive(note);
        }];
        [center addObserverForName:UIApplicationDidBecomeActiveNotification
                            object:nil
                             queue:[NSOperationQueue mainQueue]
                        usingBlock:^(NSNotification *note) {
            PrivacyDidBecomeActive(note);
        }];
    });
}

extern "C" void NBP_SetPrivacyScreenEnabled(bool enabled)
{
    s_privacyEnabled = enabled;
    PrivacyInstallObservers();
    if (!enabled)
    {
        PrivacyHide();
    }
}
#endif
