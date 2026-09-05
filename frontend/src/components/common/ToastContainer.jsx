import useToastStore from "../../stores/toastStore";

const TONES = {
  success: "bg-success-50 text-success-700 border-success-200",
  info: "bg-info-50 text-info-700 border-info-200",
  warning: "bg-warning-50 text-warning-700 border-warning-200",
};

export default function ToastContainer() {
  const toasts = useToastStore((s) => s.toasts);
  const dismiss = useToastStore((s) => s.dismiss);

  if (toasts.length === 0) return null;

  return (
    <div className="fixed top-4 right-4 z-50 flex flex-col gap-2 w-80">
      {toasts.map((t) => (
        <div
          key={t.id}
          role="alert"
          onClick={() => dismiss(t.id)}
          className={`cursor-pointer rounded-lg border px-4 py-3 shadow-md text-sm ${TONES[t.tone] || TONES.info}`}
        >
          <div className="font-semibold">{t.title}</div>
          {t.message && <div className="mt-0.5 opacity-90">{t.message}</div>}
        </div>
      ))}
    </div>
  );
}
