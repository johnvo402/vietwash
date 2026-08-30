import { ec as EC } from "elliptic";
import crypto from "crypto";

// Khởi tạo elliptic curve secp256k1 (Ethereum, Bitcoin dùng)
const ec = new EC("secp256k1");

/**
 * Lấy private key từ hex và tạo public key
 * @returns {string} - Public key dưới dạng hex
 * @throws {Error} - Nếu private key không hợp lệ hoặc không tồn tại
 */
export function generatePublicKeyFromPrivate(): string {
  const privateKeyHex = process.env.PRIVATE_KEY?.replace(/^0x/, ""); // Loại bỏ "0x" nếu có

  if (!privateKeyHex) {
    throw new Error("PRIVATE_KEY not found in .env file");
  }

  try {
    // Tạo key pair từ private key
    const keyPair = ec.keyFromPrivate(privateKeyHex, "hex");

    // Lấy public key (uncompressed - có dạng 04 + X + Y)
    const publicKey = keyPair.getPublic("hex");

    return `0x${publicKey}`;
  } catch (error) {
    throw new Error(`Invalid private key: ${error}`);
  }
}
interface SignatureResult {
  signature: string;
  nonce: string;
  timestamp: number;
  message: string;
}

/**
 * Tạo chữ ký số (signature) với nonce và timestamp
 * @returns {SignatureResult} - Chữ ký, nonce, timestamp và thông điệp đã ký
 * @throws {Error} - Nếu private key không hợp lệ hoặc không tồn tại
 */
export function generateSignature(): SignatureResult {
  // Lấy private key từ biến môi trường
  const privateKeyHex = process.env.PRIVATE_KEY?.replace(/^0x/, ""); // Loại bỏ "0x" nếu có

  if (!privateKeyHex) {
    throw new Error("PRIVATE_KEY not found in .env file");
  }

  try {
    // Khởi tạo key pair từ private key
    const keyPair = ec.keyFromPrivate(privateKeyHex, "hex");

    // Tạo nonce (mã ngẫu nhiên)
    const nonce = crypto.randomBytes(16).toString("hex");

    // Lấy timestamp hiện tại (milliseconds)
    const timestamp = Date.now();

    // Tạo thông điệp cần ký (nonce + timestamp)
    const message = `${nonce}${timestamp}`;

    // Hash message bằng SHA256 trước khi ký
    const msgHash = crypto.createHash("sha256").update(message).digest();

    // Ký message hash bằng private key
    const signature = keyPair.sign(msgHash);

    // Convert signature sang dạng hex (r + s)
    const signatureHex =
      signature.r.toString("hex") + signature.s.toString("hex");

    return {
      signature: `0x${signatureHex}`,
      nonce,
      timestamp,
      message,
    };
  } catch (error) {
    throw new Error(`Failed to generate signature: ${error}`);
  }
}
