cleanup_classification <- function(type) {
  classification_dir <- type
  
  if (dir.exists(classification_dir)) {
    unlink(classification_dir, recursive = TRUE)
  }
}