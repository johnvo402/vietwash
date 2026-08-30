import { Star } from "lucide-react";

const StarRating = ({
  averageRating,
  maxRating = 5,
}: {
  averageRating: number;
  maxRating?: number;
}) => {
  // Chuyển averageRating thành số để xử lý
  const rating = Math.min(Math.max(averageRating, 0), maxRating); // Giới hạn rating từ 0 đến maxRating
  const fullStars = Math.floor(rating); // Số ngôi sao đầy
  const hasHalfStar = rating % 1 >= 0.3 && rating % 1 <= 0.7; // Kiểm tra có hiển thị nửa ngôi sao không
  const emptyStars = maxRating - fullStars - (hasHalfStar ? 1 : 0); // Số ngôi sao rỗng

  return (
    <div className="flex items-center space-x-1">
      {/* Ngôi sao đầy */}
      {Array(fullStars)
        .fill(0)
        .map((_, index) => (
          <Star
            key={`full-${index}`}
            className="w-5 h-5 text-yellow-400 fill-yellow-400"
          />
        ))}

      {/* Nửa ngôi sao */}
      {hasHalfStar && (
        <div className="relative w-5 h-5">
          <Star className="w-5 h-5 text-gray-300" />
          <div className="absolute top-0 left-0 w-2.5 h-5 overflow-hidden">
            <Star className="w-5 h-5 text-yellow-400 fill-yellow-400" />
          </div>
        </div>
      )}

      {/* Ngôi sao rỗng */}
      {Array(emptyStars)
        .fill(0)
        .map((_, index) => (
          <Star key={`empty-${index}`} className="w-5 h-5 text-gray-300" />
        ))}

      {/* Hiển thị giá trị averageRating */}
      <span className="ml-2 text-sm font-medium text-gray-900 dark:text-white">
        {averageRating.toFixed(1)}
      </span>
    </div>
  );
};

export default StarRating;
