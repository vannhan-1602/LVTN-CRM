export default function Pagination({ pageNumber, totalPages, onPageChange }) {
  if (totalPages <= 1) return null;

  return (
    <div className="flex items-center gap-2 mt-4 justify-end">
      <button
        onClick={() => onPageChange(pageNumber - 1)}
        disabled={pageNumber <= 1}
        className="px-3 py-1 text-sm border rounded disabled:opacity-40 hover:bg-gray-50"
      >
        ‹ Trước
      </button>
      <span className="text-sm text-gray-600">
        Trang {pageNumber} / {totalPages}
      </span>
      <button
        onClick={() => onPageChange(pageNumber + 1)}
        disabled={pageNumber >= totalPages}
        className="px-3 py-1 text-sm border rounded disabled:opacity-40 hover:bg-gray-50"
      >
        Sau ›
      </button>
    </div>
  );
}
