"use client"

import { Plus, MoreHorizontal, Edit, Trash2 } from "lucide-react"
import { Button } from "@/components/ui/button"
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu"
import { CategoryTreeNode } from "../types/category"
import { useTranslations } from "next-intl"

interface CategoryNodeActionsProps {
  node: CategoryTreeNode
  onEdit: () => void
  onDelete: () => void
  onAddChild: () => void
}

export function CategoryNodeActions({ node, onEdit, onDelete, onAddChild }: CategoryNodeActionsProps) {
  const t = useTranslations();
  return (
    <div className="opacity-0 group-hover:opacity-100 transition-opacity flex items-center gap-1">
      <Button
        size="sm"
        variant="ghost"
        onClick={onAddChild}
        className="h-8 w-8 p-0 hover:bg-primary-foreground"
        title={t("category.addChild")}
      >
        <Plus className="w-4 h-4 text-green-600" />
      </Button>

      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button size="sm" variant="ghost" className="h-8 w-8 p-0 hover:bg-primary-foreground">
            <MoreHorizontal className="w-4 h-4" />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
          <DropdownMenuItem onClick={onEdit} className="flex items-center gap-2">
            <Edit className="w-4 h-4" />
            {t("common.edit")}
          </DropdownMenuItem>
          <DropdownMenuItem onClick={onDelete} className="flex items-center gap-2 text-red-600 hover:text-red-700">
            <Trash2 className="w-4 h-4" />
            {t("common.delete")}
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  )
}
