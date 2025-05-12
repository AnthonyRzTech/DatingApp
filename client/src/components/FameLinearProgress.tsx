import { Box, Typography } from '@mui/material';
import { styled } from '@mui/material/styles';
import LinearProgress, { linearProgressClasses } from '@mui/material/LinearProgress';

const StyledLinearProgress = styled(LinearProgress)(({ theme }) => ({
  height: 50,
  borderRadius: 5,
  [`& .${linearProgressClasses.bar}`]: {
    borderRadius: 5,
  }
}));

interface FameLinearProgressProps {
  value: number;
  variant: "determinate";
}

export const FameLinearProgress = (props: FameLinearProgressProps) => {
  return (
    <Box position="relative" display="flex" alignItems="center">
      <StyledLinearProgress 
        variant={props.variant} 
        value={props.value} 
        sx={{ width: '100%' }}
      />
      <Typography
        variant="body1"
        color="text.primary"
        sx={{
          position: 'absolute',
          left: '50%',
          transform: 'translateX(-50%)',
        }}
      >
        {`Rating: ${Math.round(props.value)}%`}
      </Typography>
    </Box>
  );
};