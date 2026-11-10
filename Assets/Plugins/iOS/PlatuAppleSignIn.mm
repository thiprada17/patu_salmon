#import <AuthenticationServices/AuthenticationServices.h>
#import <CommonCrypto/CommonDigest.h>
#import <UIKit/UIKit.h>

extern "C" void UnitySendMessage(const char *obj, const char *method, const char *msg);

@interface PlatuAppleSignInDelegate : NSObject <ASAuthorizationControllerDelegate, ASAuthorizationControllerPresentationContextProviding>
@property(nonatomic, copy) NSString *gameObjectName;
@property(nonatomic, strong) ASAuthorizationController *authorizationController;
@end

@implementation PlatuAppleSignInDelegate

- (ASPresentationAnchor)presentationAnchorForAuthorizationController:(ASAuthorizationController *)controller {
    for (UIScene *scene in UIApplication.sharedApplication.connectedScenes) {
        if (![scene isKindOfClass:[UIWindowScene class]]) continue;
        UIWindowScene *windowScene = (UIWindowScene *)scene;
        for (UIWindow *window in windowScene.windows) {
            if (window.isKeyWindow) return window;
        }
    }
    return UIApplication.sharedApplication.keyWindow;
}

- (void)sendResult:(NSDictionary *)result {
    NSError *error = nil;
    NSData *data = [NSJSONSerialization dataWithJSONObject:result options:0 error:&error];
    NSString *json = error ? @"{\"error\":\"Could not serialize Apple sign-in result\"}" : [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
    UnitySendMessage(self.gameObjectName.UTF8String, "OnNativeAppleSignIn", json.UTF8String);
    self.authorizationController.delegate = nil;
    self.authorizationController.presentationContextProvider = nil;
}

- (void)authorizationController:(ASAuthorizationController *)controller didCompleteWithAuthorization:(ASAuthorization *)authorization {
    ASAuthorizationAppleIDCredential *credential = (ASAuthorizationAppleIDCredential *)authorization.credential;
    NSData *tokenData = credential.identityToken;
    NSString *identityToken = tokenData ? [[NSString alloc] initWithData:tokenData encoding:NSUTF8StringEncoding] : nil;
    if (identityToken.length == 0) {
        [self sendResult:@{@"error": @"Apple did not return an identity token"}];
        return;
    }

    NSMutableDictionary *result = [@{@"identity_token": identityToken} mutableCopy];
    if (credential.email) result[@"email"] = credential.email;
    if (credential.fullName.givenName) result[@"given_name"] = credential.fullName.givenName;
    if (credential.fullName.familyName) result[@"family_name"] = credential.fullName.familyName;
    [self sendResult:result];
}

- (void)authorizationController:(ASAuthorizationController *)controller didCompleteWithError:(NSError *)error {
    if (error.code == ASAuthorizationErrorCanceled) {
        [self sendResult:@{@"error": @"Sign in with Apple was cancelled"}];
        return;
    }
    [self sendResult:@{@"error": error.localizedDescription ?: @"Apple sign-in failed"}];
}

@end

static PlatuAppleSignInDelegate *gPlatuAppleSignInDelegate;

extern "C" void PlatuAppleSignIn(const char *gameObjectName, const char *rawNonce) {
    NSString *objectName = gameObjectName ? [NSString stringWithUTF8String:gameObjectName] : @"AppleController";
    NSString *nonce = rawNonce ? [NSString stringWithUTF8String:rawNonce] : @"";
    if (nonce.length == 0) {
        UnitySendMessage(objectName.UTF8String, "OnNativeAppleSignIn", "{\"error\":\"Nonce is missing\"}");
        return;
    }

    NSData *nonceData = [nonce dataUsingEncoding:NSUTF8StringEncoding];
    uint8_t digest[CC_SHA256_DIGEST_LENGTH];
    CC_SHA256(nonceData.bytes, (CC_LONG)nonceData.length, digest);
    NSMutableString *hashedNonce = [NSMutableString stringWithCapacity:CC_SHA256_DIGEST_LENGTH * 2];
    for (int index = 0; index < CC_SHA256_DIGEST_LENGTH; index++)
        [hashedNonce appendFormat:@"%02x", digest[index]];

    dispatch_async(dispatch_get_main_queue(), ^{
        ASAuthorizationAppleIDProvider *provider = [[ASAuthorizationAppleIDProvider alloc] init];
        ASAuthorizationAppleIDRequest *request = [provider createRequest];
        request.requestedScopes = @[ASAuthorizationScopeEmail, ASAuthorizationScopeFullName];
        request.nonce = hashedNonce;

        gPlatuAppleSignInDelegate = [[PlatuAppleSignInDelegate alloc] init];
        gPlatuAppleSignInDelegate.gameObjectName = objectName;
        ASAuthorizationController *controller = [[ASAuthorizationController alloc] initWithAuthorizationRequests:@[request]];
        gPlatuAppleSignInDelegate.authorizationController = controller;
        controller.delegate = gPlatuAppleSignInDelegate;
        controller.presentationContextProvider = gPlatuAppleSignInDelegate;
        [controller performRequests];
    });
}

extern "C" void PlatuShowEmailConfirmationPrompt(void) {
    dispatch_async(dispatch_get_main_queue(), ^{
        UIWindow *keyWindow = nil;
        for (UIScene *scene in UIApplication.sharedApplication.connectedScenes) {
            if (![scene isKindOfClass:[UIWindowScene class]]) continue;
            UIWindowScene *windowScene = (UIWindowScene *)scene;
            for (UIWindow *window in windowScene.windows) {
                if (window.isKeyWindow) {
                    keyWindow = window;
                    break;
                }
            }
            if (keyWindow) break;
        }

        UIViewController *presenter = keyWindow.rootViewController;
        while (presenter.presentedViewController)
            presenter = presenter.presentedViewController;
        if (!presenter) return;

        UIAlertController *alert = [UIAlertController
            alertControllerWithTitle:@"ยืนยันอีเมล"
            message:@"กรุณาเปิด Gmail แล้วกดลิงก์ยืนยัน จากนั้นกลับมาที่เกม"
            preferredStyle:UIAlertControllerStyleAlert];

        [alert addAction:[UIAlertAction actionWithTitle:@"ไว้ทีหลัง"
            style:UIAlertActionStyleCancel handler:nil]];
        [alert addAction:[UIAlertAction actionWithTitle:@"เปิด Gmail"
            style:UIAlertActionStyleDefault handler:^(__unused UIAlertAction *action) {
                NSURL *gmailURL = [NSURL URLWithString:@"googlegmail://"];
                [[UIApplication sharedApplication] openURL:gmailURL options:@{}
                    completionHandler:nil];
            }]];

        [presenter presentViewController:alert animated:YES completion:nil];
    });
}
