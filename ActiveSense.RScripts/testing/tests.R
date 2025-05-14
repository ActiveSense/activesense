# ==================================
# REGRESSION TESTING
# ==================================

run_regression <- function(test_description, use_mmap) {
  test_that(test_description, {
    
    # Sets option
    options(myapp_use_mmap = use_mmap)
    
    # make sure to reset the option
    on.exit({
      options(myapp_use_mmap = NULL)
    }, add = TRUE)
    
    print("WORKING DIRECTORY BEFORE")
    print(getwd())
    
    # set working directory
    setwd(dirname(getwd()))
    
    print("WORKING DIRECTORY AFTER")
    print(getwd())
    
    # clean up
    directories <- c(
      file.path(getwd(), "data"),
      file.path(getwd(), "outputs")
    )
    
    for (dir in directories) {
      unlink(list.files(dir, full.names = TRUE, recursive = FALSE), recursive = TRUE)
    }
    
    # copying bin files into /data/ folder
    files_to_copy <- list.files("testing/references/bin", full.names = TRUE)
    file.copy(from = files_to_copy, to = "data")
    
    # run analysis
    main_script_path <- file.path(getwd(), "_main.R")
    source(main_script_path)
    
    # compare newly generated files with the correct csv folder
    output_dir <- file.path(getwd(), "/outputs")
    correct_csv_dir <- file.path(getwd(), "/testing/references/golden_tickets")
    
    # Get list of files in output directory
    output_files <- list.files(output_dir, pattern = "\\.csv$", full.names = FALSE, recursive = TRUE)
    
    # Loop through each output file and compare with reference
    for (file_name in output_files) {
      # Read csv files
      output_data <- read.csv(file.path(output_dir, file_name), stringsAsFactors = FALSE)
      reference_data <- read.csv(file.path(correct_csv_dir, file_name), stringsAsFactors = FALSE)
      
      # Compare content
      comparison <- all.equal(output_data, reference_data)
      
      # Assert equality
      expect_true(isTRUE(comparison),
                  info = paste("Files differ:", file_name, "-", comparison))
    }
    
  })
  
}

run_regression("Regression test: run with mmap enabled", TRUE)

run_regression("Regression test: run with mmap disabled", FALSE)