cleanup_classification <- function() {
  classification_dir <- "GENEAclassification"
  
  if (dir.exists(classification_dir)) {
    unlink(classification_dir, recursive = TRUE)
  }
}