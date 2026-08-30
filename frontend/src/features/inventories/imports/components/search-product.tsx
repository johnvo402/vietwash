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

interface SearchableSelectProps {
  options: { value: string; label: string }[];
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  isLoading?: boolean;
  error?: string;
  fetchNextPage?: () => void;
  hasNextPage?: boolean;
  onSearch?: (searchTerm: string) => void;
}

export default function SearchableSelect({
  options,
  value,
  onChange,
  placeholder,
  isLoading,
  error,
  fetchNextPage,
  hasNextPage,
  onSearch,
}: SearchableSelectProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [debouncedSearchTerm] = useDebounce(searchTerm, 300);
  const scrollRef = useRef<HTMLDivElement>(null);

  // Trigger search when debounced search term changes
  useEffect(() => {
    if (onSearch) {
      onSearch(debouncedSearchTerm);
    }
  }, [debouncedSearchTerm, onSearch]);

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
  const selectedOption = options.find((option) => option.value === value);

  return (
    <Popover open={isOpen} onOpenChange={setIsOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          role="combobox"
          className="w-full justify-between"
          disabled={isLoading}
        >
          {isLoading
            ? "Loading..."
            : error
              ? error
              : selectedOption
                ? selectedOption.label
                : placeholder || "Select an option"}
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
                  key={option.value}
                  className="flex items-center p-2 hover:bg-accent cursor-pointer rounded-md"
                  onClick={() => {
                    onChange(option.value);
                    setIsOpen(false);
                    setSearchTerm("");
                    if (onSearch) {
                      onSearch("");
                    }
                  }}
                >
                  <Check
                    className={`mr-2 h-4 w-4 ${option.value === value ? "opacity-100" : "opacity-0"}`}
                  />
                  {option.label}
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
