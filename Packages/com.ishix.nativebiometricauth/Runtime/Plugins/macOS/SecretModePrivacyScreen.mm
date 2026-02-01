#import <TargetConditionals.h>
#if TARGET_OS_OSX
#import <Cocoa/Cocoa.h>
static bool s_privacyEnabled = false;
static NSWindow *s_privacyWindow = nil;

static NSWindow *PrivacyMainWindow(void)
{
    NSWindow *window = [NSApp mainWindow];
    if (window != nil)
    {
        return window;
    }
    return [NSApp keyWindow];
}

static NSScreen *PrivacyTargetScreen(void)
{
    NSWindow *window = PrivacyMainWindow();
    if (window != nil && window.screen != nil)
    {
        return window.screen;
    }
    return [NSScreen mainScreen];
}

static void PrivacyEnsureWindow(void)
{
    if (s_privacyWindow != nil)
    {
        return;
    }
    NSScreen *screen = PrivacyTargetScreen();
    NSRect frame = screen ? screen.frame : NSMakeRect(0, 0, 800, 600);
    s_privacyWindow = [[NSWindow alloc] initWithContentRect:frame
                                                  styleMask:NSWindowStyleMaskBorderless
                                                    backing:NSBackingStoreBuffered
                                                      defer:NO];
    [s_privacyWindow setLevel:NSScreenSaverWindowLevel];
    [s_privacyWindow setOpaque:NO];
    [s_privacyWindow setBackgroundColor:[NSColor clearColor]];
    [s_privacyWindow setIgnoresMouseEvents:YES];
    [s_privacyWindow setReleasedWhenClosed:NO];
    [s_privacyWindow setCollectionBehavior:(NSWindowCollectionBehaviorCanJoinAllSpaces
                                            | NSWindowCollectionBehaviorFullScreenAuxiliary)];

    NSVisualEffectView *blur = [[NSVisualEffectView alloc] initWithFrame:frame];
    blur.autoresizingMask = NSViewWidthSizable | NSViewHeightSizable;
    blur.material = NSVisualEffectMaterialUnderWindowBackground;
    blur.state = NSVisualEffectStateActive;
    s_privacyWindow.contentView = blur;
}

static void PrivacyShowIfNeeded(void)
{
    if (!s_privacyEnabled)
    {
        return;
    }
    PrivacyEnsureWindow();
    NSScreen *screen = PrivacyTargetScreen();
    if (screen != nil)
    {
        [s_privacyWindow setFrame:screen.frame display:NO];
    }
    [s_privacyWindow orderFrontRegardless];
}

static void PrivacyHide(void)
{
    [s_privacyWindow orderOut:nil];
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
        [center addObserverForName:NSApplicationWillResignActiveNotification
                            object:nil
                             queue:[NSOperationQueue mainQueue]
                        usingBlock:^(NSNotification *note) {
            PrivacyWillResignActive(note);
        }];
        [center addObserverForName:NSApplicationDidBecomeActiveNotification
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
