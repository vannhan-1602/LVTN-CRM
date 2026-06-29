import { ChevronLeft, ChevronRight } from "lucide-react";

export default function Pagination({ pageNumber, totalPages, onPageChange }) {
  if (totalPages <= 1) return null;

  return (
    <div className="flex items-center gap-3">
      <button
        onClick={() => onPageChange(pageNumber - 1)}
        disabled={pageNumber <= 1}
        className="inline-flex items-center gap-1 px-3 py-1.5 text-xs font-medium border border-ink-200 rounded-lg disabled:opacity-40 hover:bg-ink-100 disabled:cursor-not-allowed text-ink-700"
      >
        <ChevronLeft size={14} /> Trước
      </button>

      <span className="text-xs text-ink-500">
        Trang <span className="font-semibold text-ink-900">{pageNumber}</span> /{" "}
        {totalPages}
      </span>

      <button
        onClick={() => onPageChange(pageNumber + 1)}
        disabled={pageNumber >= totalPages}
        className="inline-flex items-center gap-1 px-3 py-1.5 text-xs font-medium border border-ink-200 rounded-lg disabled:opacity-40 hover:bg-ink-100 disabled:cursor-not-allowed text-ink-700"
      >
        Sau <ChevronRight size={14} />
      </button>
    </div>
  );
}
