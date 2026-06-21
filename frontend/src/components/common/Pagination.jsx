export default function Pagination({ pageNumber, totalPages, onPageChange }) {
  if (totalPages <= 1) return null;

  return (
    <div className="flex items-center gap-2">
      <button
        onClick={() => onPageChange(pageNumber - 1)}
        disabled={pageNumber <= 1}
        className="px-3 py-1 text-sm border rounded-lg disabled:opacity-40 hover:bg-gray-50 disabled:cursor-not-allowed"
      >
        ← Trước
      </button>

      <span className="text-sm text-gray-600">
        Trang <strong>{pageNumber}</strong> / {totalPages}
      </span>

      <button
        onClick={() => onPageChange(pageNumber + 1)}
        disabled={pageNumber >= totalPages}
        className="px-3 py-1 text-sm border rounded-lg disabled:opacity-40 hover:bg-gray-50 disabled:cursor-not-allowed"
      >
        Sau →
      </button>
    </div>
  );
}
