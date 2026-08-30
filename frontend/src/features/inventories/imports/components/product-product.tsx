import { useState, useEffect, useRef } from "react";
import { Input } from "@/components/ui/input";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Button } from "@/components/ui/button";
import { Check, ChevronDown } from "lucide-react";
import { useDebounce } from "@/hooks/use-debounce";
import { ListBranchProductResponse } from "@/api/generated";
import Image from "next/image";

interface ProductSelectProps {
  options: ListBranchProductResponse[];
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  isLoading?: boolean;
  error?: string;
  fetchNextPage?: () => void;
  hasNextPage?: boolean;
  onSearch?: (searchTerm: string) => void;
}

export default function ProductSelect({
  options,
  value,
  onChange,
  placeholder,
  isLoading,
  error,
  fetchNextPage,
  hasNextPage,
  onSearch,
}: ProductSelectProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const debouncedSearchTerm = useDebounce(searchTerm, 300);
  const scrollRef = useRef<HTMLDivElement>(null);

  // Trigger search when debounced search term changes
  useEffect(() => {
    if (onSearch) {
      onSearch(debouncedSearchTerm);
    }
  }, [debouncedSearchTerm, onSearch]);

  // Reset search when popover closes
  useEffect(() => {
    if (!isOpen) {
      setSearchTerm("");
      if (onSearch) {
        onSearch("");
      }
    }
  }, [isOpen, onSearch]);

  // Focus input when popover opens
  const inputRef = useRef<HTMLInputElement>(null);
  useEffect(() => {
    if (isOpen && inputRef.current) {
      inputRef.current.focus();
    }
  }, [isOpen]);

  // Infinite scroll handler
  const handleScroll = () => {
    if (scrollRef.current && fetchNextPage && hasNextPage && !isLoading) {
      const { scrollTop, scrollHeight, clientHeight } = scrollRef.current;
      if (scrollTop + clientHeight >= scrollHeight - 10) {
        fetchNextPage();
      }
    }
  };

  // Get the selected option's label for display
  const selectedOption = options.find(
    (option) => option.id?.toString() === value
  );

  return (
    <Popover open={isOpen} onOpenChange={setIsOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          role="combobox"
          className="w-full justify-between"
          disabled={isLoading}
        >
          <div className="flex items-center space-x-2">
            {isLoading ? (
              "Loading..."
            ) : error ? (
              error
            ) : selectedOption ? (
              <>
                <Image
                  src={selectedOption.image ?? "/logo/favicon.svg"}
                  alt={selectedOption.name ?? ""}
                  className="h-6 w-6 rounded mr-2 object-cover"
                  width={24}
                  height={24}
                  onError={(e) => (e.currentTarget.style.display = "none")} // Hide image on error
                />
                <span>{selectedOption.name}</span>
              </>
            ) : (
              placeholder || "Select an option"
            )}
          </div>
          <ChevronDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[300px] p-0">
        <div className="p-2">
          <Input
            ref={inputRef}
            placeholder="Search..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="mb-2"
          />
          <div
            ref={scrollRef}
            className="max-h-[200px] overflow-y-auto"
            onScroll={handleScroll}
          >
            {isLoading && (
              <div className="p-2 text-sm text-muted-foreground">
                Loading...
              </div>
            )}
            {error && (
              <div className="p-2 text-sm text-destructive">{error}</div>
            )}
            {!isLoading && !error && options.length === 0 && (
              <div className="p-2 text-sm text-muted-foreground">
                No results found
              </div>
            )}
            {!isLoading &&
              !error &&
              options.map((option) => (
                <div
                  key={option.id?.toString()}
                  className="flex items-center p-2 hover:bg-accent cursor-pointer rounded-md"
                  onClick={() => {
                    onChange(option.id?.toString()!);
                    setIsOpen(false);
                    setSearchTerm("");
                    if (onSearch) {
                      onSearch("");
                    }
                  }}
                >
                  <Check
                    className={`mr-2 h-4 w-4 ${option.id?.toString() === value ? "opacity-100" : "opacity-0"}`}
                  />
                  <Image
                    src={option.image ?? "/logo/favicon.svg"}
                    alt={option.name ?? ""}
                    className="h-6 w-6 rounded mr-2 object-cover"
                    width={24}
                    height={24}
                    onError={(e) => (e.currentTarget.style.display = "none")}
                  />
                  <div className="flex-1 flex flex-col">
                    <span className="text-sm font-medium">{option.name}</span>
                    <span className="text-xs text-muted-foreground">
                      {option.sku}
                    </span>
                  </div>
                  <span className="text-sm text-muted-foreground ml-auto">
                    {option.stockQuantity}{" "}
                    {option.unitRelations?.find((x) => x.baseUnit)?.name}
                  </span>
                </div>
              ))}
            {hasNextPage && !isLoading && (
              <div className="p-2 text-sm text-muted-foreground text-center">
                Scroll to load more...
              </div>
            )}
          </div>
        </div>
      </PopoverContent>
    </Popover>
  );
}
