CREATE OR ALTER PROCEDURE [dbo].[UserAsymmetricKeys_Regenerate]
    @UserId UNIQUEIDENTIFIER OUTPUT,
    @PublicKey VARCHAR(MAX),
    @PrivateKey VARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON

    UPDATE [dbo].[User]
    SET [PublicKey] = @PublicKey,
        [PrivateKey] = @PrivateKey
    WHERE [Id] = @UserId
END
