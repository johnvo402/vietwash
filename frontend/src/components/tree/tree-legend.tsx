import { Folder, Package, Plus, Edit, Trash2 } from "lucide-react"
import { useTranslations } from "next-intl"

export function TreeLegend() {
  const t = useTranslations()
  return (
    <div className="flex flex-wrap gap-4 p-3 bg-gray-50 rounded-lg text-xs">
      <div className="flex items-center gap-2">
        <Folder className="w-4 h-4 text-amber-600" />
        <span>{t("category.parent")}</span>
      </div>
      <div className="flex items-center gap-2">
        <Package className="w-4 h-4 text-blue-500" />
        <span>{t("category.leaf")}</span>
      </div>
      <div className="flex items-center gap-2">
        <Plus className="w-4 h-4 text-green-600" />
        <span>{t("category.addChild")}</span>
      </div>
      <div className="flex items-center gap-2">
        <Edit className="w-4 h-4 text-blue-600" />
        <span>{t("common.edit")}</span>
      </div>
      <div className="flex items-center gap-2">
        <Trash2 className="w-4 h-4 text-red-600" />
        <span>{t("common.delete")}</span>
      </div>
    </div>
  )
}
