using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
namespace OracleCMS.Common.Utility.Helpers
{
    public static class EncryptionHelper
    {
        public static string EncryptPassword(string password, string key)
		{
			if (string.IsNullOrEmpty(password)) { return ""; }
			byte[] encryptedBytes;
			byte[] iv;
			var keyByte = DeriveKeyByte(key);
			using (Aes aes = Aes.Create())
			{
				aes.Key = keyByte;
				aes.GenerateIV();  // Generate a secure, random IV
				iv = aes.IV;
				using ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
				byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
				encryptedBytes = encryptor.TransformFinalBlock(passwordBytes, 0, passwordBytes.Length);
			}
			// Prepend the IV to the encrypted data
			byte[] combinedData = new byte[iv.Length + encryptedBytes.Length];
			Buffer.BlockCopy(iv, 0, combinedData, 0, iv.Length);
			Buffer.BlockCopy(encryptedBytes, 0, combinedData, iv.Length, encryptedBytes.Length);
			return Convert.ToBase64String(combinedData);
		}
		public static string DecryptPassword(string encryptedPassword, string key)
		{
			if (string.IsNullOrEmpty(encryptedPassword)) { return ""; }
			byte[] combinedData = Convert.FromBase64String(encryptedPassword);
			byte[] decryptedBytes;
			var keyByte = DeriveKeyByte(key);
			using (Aes aes = Aes.Create())
			{
				byte[] iv = new byte[aes.BlockSize / 8];  // Extract the IV
				byte[] encryptedBytes = new byte[combinedData.Length - iv.Length];
				Buffer.BlockCopy(combinedData, 0, iv, 0, iv.Length);
				Buffer.BlockCopy(combinedData, iv.Length, encryptedBytes, 0, encryptedBytes.Length);
				aes.Key = keyByte;
				aes.IV = iv;
				using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
				decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
			}
			return Encoding.UTF8.GetString(decryptedBytes);
		}
		private static byte[] DeriveKeyByte(string key)
		{
			return SHA256.HashData(Encoding.UTF8.GetBytes(key));
		}
		public static string HashPassword(string password)
		{
			// Generate a random salt
			byte[] salt = new byte[128 / 8];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(salt);
			}

			// Hash the password using PBKDF2
			string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
				password: password,
				salt: salt,
				prf: KeyDerivationPrf.HMACSHA256,
				iterationCount: 100000,
				numBytesRequested: 256 / 8));

			// Combine version, salt, and hashed password (stored in Base64 format)
			byte[] version = [0x01]; // Example version byte
			byte[] combinedHash = new byte[version.Length + salt.Length + Convert.FromBase64String(hashed).Length];

			Buffer.BlockCopy(version, 0, combinedHash, 0, version.Length);
			Buffer.BlockCopy(salt, 0, combinedHash, version.Length, salt.Length);
			Buffer.BlockCopy(Convert.FromBase64String(hashed), 0, combinedHash, version.Length + salt.Length, Convert.FromBase64String(hashed).Length);

			// Return the Base64 encoded combined hash
			return Convert.ToBase64String(combinedHash);
		}
		public static bool VerifyPassword(string hashedPassword, string providedPassword)
		{
			// Decode the stored hash
			byte[] decodedHash = Convert.FromBase64String(hashedPassword);

			// Extract the version, salt, and stored hash value
			byte[] version = new byte[1];
			byte[] salt = new byte[128 / 8];
			byte[] storedHash = new byte[256 / 8];

			Buffer.BlockCopy(decodedHash, 0, version, 0, version.Length);
			Buffer.BlockCopy(decodedHash, version.Length, salt, 0, salt.Length);
			Buffer.BlockCopy(decodedHash, version.Length + salt.Length, storedHash, 0, storedHash.Length);

			// Hash the provided password using the extracted salt and iteration count
			string providedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
				password: providedPassword,
				salt: salt,
				prf: KeyDerivationPrf.HMACSHA256,
				iterationCount: 100000,
				numBytesRequested: 256 / 8));

			// Compare the stored hash with the newly generated hash
			return Convert.FromBase64String(providedHash).SequenceEqual(storedHash);
		}
    }
}
