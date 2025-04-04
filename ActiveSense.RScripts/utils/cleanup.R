cleanup_classification <- function() {
  classification_dir <- list("ActivityClassification", "SleepClassification")
  
  for (dir in classification_dir) {
    if (dir.exists(dir)) {
      unlink(dir, recursive = TRUE)
    } 
  }
}